using Mirror;
using UnityEngine;

namespace Fifbox.Player
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerPhysics : NetworkBehaviour
    {
        [Header("Objects")]
        [SerializeField] protected Rigidbody _rigidbody;
        [SerializeField] protected Transform _orientation;
        [SerializeField] protected Collider _collider;

        [Header("Move")]
        [SerializeField] protected float _movementSpeed;

        // это для сглаживания мувмент инпута
        // на земле игрока будет легче контролировать чем в воздухе, по этому сглаживание в воздухе должно быть выше чем на земле
        [SerializeField] protected float _groundMovementSmoothing;
        [SerializeField] protected float _airMovementSmoothing;

        [Space(9)]
        [SerializeField] protected AnimationCurve _accelerationCurve;
        [SerializeField, Min(0.01f)] protected float _accelerationDuration;

        [Space(9)]
        [SerializeField] protected AnimationCurve _decelerationCurve;
        [SerializeField, Min(0.01f)] protected float _decelerationDuration;

        // не сглаженный мувмент инпут (аналог Input.GetAxisRaw())
        // мувмент инпут контролируется в вдругом классе как и некоторые методы типа прыжка
        // это надо чтобы потом можно было легко создать типа ии игроков или прочей хуйни
        // lambda players это пиздец веселая хуйня в гмоде и я не хочу чтоб моддеры переписывали всю физику игрока чтобы сделать чота подобное
        protected Vector2 _movementInput;

        // сглаженный мувмент инпут (аналог Input.GetAxis())
        private Vector2 _smoothedMovementInput;

        // последний сглаженный мувмент инпут который не равен нулю (нужен для деселерации)
        private Vector2 _lastSmoothedMovementInput;

        // это короче SmoothDamp здесь велосити хранит для сглаживания мувмент инпута
        private Vector2 _movementInputSmoothingVelocity;

        // прошедшее время с начала акселерации
        private float _accelerationTime;

        // значение акселерации полученое из курвы
        protected float _accelerationValue;

        // ну тут аналогично все
        private float _decelerationTime;
        protected float _decelerationValue;

        private bool _accelerating;

        [Header("Jump")]
        [SerializeField] protected float _jumpForce;

        // реализации жамп баффера щас нет никакой я потом её сделаю
        [SerializeField] protected float _jumpBufferTime;

        private float _jumpBufferTimer;

        [Header("Map Check")]
        [SerializeField] protected LayerMask _mapLayers;

        [Space(9)]
        [SerializeField] protected Vector3 _groundCheckSize;
        [SerializeField] protected Vector3 _groundCheckOffset;

        protected Vector3 GroundCheckPosition => transform.position + _groundCheckOffset;

        protected bool _grounded;
        protected Vector3 _groundNormal;
        protected float _groundAngle;

        [Header("Slope Handling")]
        [SerializeField] protected float _slopeAngleLimit;

        protected Vector3 _velocity;
        private Vector3 _velocityFromGravity;

        protected virtual void OnStart() { }
        protected virtual void OnUpdate() { }

        // автоматом находит ригидбади на объекте и задает настройки нужные
        protected override void OnValidate()
        {
            base.OnValidate();

            if (TryGetComponent(out _rigidbody))
            {
                _rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
                _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
                _rigidbody.freezeRotation = true;
                _rigidbody.isKinematic = false;
                _rigidbody.useGravity = false;
            }
        }

        private void Start()
        {
            // если ну короче мне лень комент писать ты итак понимаешь зачем оно надо
            // короче ты нихуя не понял блять
            // это чтобы иссуе не было в мультиплеере когда чужие клиенты вызывают метод который они не должны вызывать
            // и все ломается нахуй
            if (!isLocalPlayer) return;
            OnStart();
        }

        private void Update()
        {
            if (!isLocalPlayer) return;

            CheckGrounded();

            // честно я щас по логике просто отмодифицировал код из ишки чтобы он более физическим был
            // я не уверен что оно будет работат...
            if (_grounded && _velocityFromGravity.magnitude != 0 && _groundAngle <= _slopeAngleLimit)
            {
                _velocity -= _velocityFromGravity;
                _velocityFromGravity = Vector3.zero;
            }
            else
            {
                _velocity += Physics.gravity * Time.deltaTime;
                _velocityFromGravity += Physics.gravity * Time.deltaTime;
            }

            OnUpdate();

            _smoothedMovementInput = Vector2.SmoothDamp
            (
                _smoothedMovementInput,
                _movementInput,
                ref _movementInputSmoothingVelocity,
                _grounded ? _groundMovementSmoothing : _airMovementSmoothing
            );

            if (_movementInput.magnitude > 0)
            {
                _lastSmoothedMovementInput = _smoothedMovementInput;
            }

            Acceleration();
            Deceleration();

            _accelerationValue = _accelerationCurve.Evaluate(_accelerationTime / _accelerationDuration);
            _decelerationValue = _decelerationCurve.Evaluate(_decelerationTime / _decelerationDuration);
        }

        // пизда :overdrive_shakal:
        protected void CheckGrounded()
        {
            // сначала коробкой чекаем вообще ли мы на земле, а потом уже лучем находим слоуп
            _grounded = Physics.CheckBox(GroundCheckPosition, _groundCheckSize / 2, Quaternion.identity, _mapLayers, QueryTriggerInteraction.Ignore);
            if (_grounded)
            {
                // короче оно умно позиционирует луч на землю
                // чтобы когда игрок с плоской поверхности на слоуп переходит было все без заметных изменений в скорости
                var radius = _collider.transform.lossyScale.x / 2;
                var yOffset = _collider.transform.lossyScale.y / 2;

                var direction = _rigidbody.linearVelocity.y > 0
                    ? _orientation.forward * _movementInput.y + _orientation.right * _movementInput.x
                    : Vector3.zero;

                var position = new Vector3
                (
                    transform.position.x + direction.x * radius,
                    transform.position.y + yOffset,
                    transform.position.z + direction.z * radius
                );

                // ну тут рейкастим и получаем инфу о поверхности
                Physics.Raycast(position, Vector3.down, out RaycastHit hitInfo, yOffset + 0.5f, _mapLayers);
                _groundNormal = hitInfo.normal;
                _groundAngle = Mathf.Round(Vector3.Angle(Vector3.up, _groundNormal) * 10) / 10f;
            }
            else
            {
                _groundNormal = Vector3.up;
                _groundAngle = 0;
            }
        }

        private void Acceleration()
        {
            if (_accelerating && (_movementInput.magnitude == 0 || _accelerationTime >= _accelerationDuration))
            {
                _accelerating = false;
            }

            if (_movementInput.magnitude > 0 && !_accelerating)
            {
                _accelerating = true;
                _accelerationTime = _decelerationValue * _decelerationDuration;
            }

            if (_accelerating)
            {
                _accelerationTime += Time.deltaTime;
            }
        }

        private void Deceleration()
        {
            if (_movementInput.magnitude > 0)
                _decelerationTime = 0;
            else if (_decelerationTime < _decelerationDuration)
                _decelerationTime += Time.deltaTime;
        }

        // наконецта двигаем игрока
        private void FixedUpdate()
        {
            if (!isLocalPlayer) return;

            // нужно юзать последний инпут чтобы работала деселерация
            var direction = _orientation.forward * _lastSmoothedMovementInput.y + _orientation.right * _lastSmoothedMovementInput.x;

            // для более пиздатого передвижения по слоупам
            var moveDirection = _groundAngle <= _slopeAngleLimit ? Vector3.ProjectOnPlane(direction, _groundNormal) : direction;

            // бустим игрока на слоупах для ещё более пиздатого передвижения
            // причем буст на слоупах очень умно расчитывается
            // если вот у тебя слоуп наклонен по X
            // тогда ты будешь быстрее бежать по Z но по X будешь с обычной скоростью
            var angleX = Vector2.Angle(Vector2.up, new(_groundNormal.x, _groundNormal.y));
            var angleZ = Vector2.Angle(Vector2.up, new(_groundNormal.z, _groundNormal.y));
            var angleBoost = new Vector3
            (
                1 + angleX * angleX / 3600,
                1 + _groundAngle * _groundAngle / 3600,
                1 + angleZ * angleZ / 3600
            );

            var speed = _accelerationValue * _decelerationValue * _movementSpeed;
            _rigidbody.linearVelocity = new Vector3
            (
                speed * angleBoost.x * moveDirection.x,
                speed * angleBoost.y * moveDirection.y,
                speed * angleBoost.z * moveDirection.z
            ) + _velocity + angleBoost;
        }

        // нужно чтобы когда игрок башкой бился об потолок его скорость по Y ресетало
        private void OnCollisionEnter(Collision other)
        {
            foreach (var contact in other.contacts)
            {
                if (contact.normal.y <= -0.5f) _velocity.y = 0;
            }
        }

        protected void TryJump()
        {
            if (!_grounded) return;
            _velocity.y += _jumpForce;
        }
    }
}