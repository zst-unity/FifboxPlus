using System;
using System.IO;
using Newtonsoft.Json.Linq;
using UnityEngine;
using ZSToolkit.GlobalData.Extensions;

namespace ZSToolkit.GlobalData
{
    public static class GlobalData
    {
        /* NOTE: Shit is not used. */
        public static readonly string dataPath = $"{Application.persistentDataPath}/data";

        private static void DeleteFile(string path)
        {
            File.Delete(path);

            var directory = Path.GetDirectoryName(path).Replace(@"\", "/");
            if (directory == Application.persistentDataPath || directory == dataPath) return;

            if (Directory.GetFiles(directory, "*", SearchOption.AllDirectories).Length == 0) Directory.Delete(directory);
        }

        /// <summary>
        /// creates a new .globaldata file
        /// </summary>
        /// <param name="path">local path of .globaldata file</param>
        /// <exception cref="ArgumentException"></exception>
        public static void New(string path)
        {
            if (string.IsNullOrEmpty(path)) throw new ArgumentException("path cannot be null or empty");

            var jsonPath = $"";
            Directory.CreateDirectory(Path.GetDirectoryName(jsonPath));
            File.WriteAllText(jsonPath, "{}");
        }

        /// <summary>
        /// erases every .globaldata file
        /// </summary>
        public static void Erase()
        {
            foreach (var file in Directory.GetFiles(dataPath, "*.globaldata", SearchOption.AllDirectories))
            {
                DeleteFile(file);
            }
        }

        /// <summary>
        /// erases .globaldata file
        /// </summary>
        /// <param name="path">local path of .globaldata file</param>
        /// <exception cref="ArgumentException"></exception>
        public static void Erase(string path)
        {
            if (string.IsNullOrEmpty(path)) throw new ArgumentException("path cannot be null or empty");

            if (!path.EndsWith("/"))
            {
                var jsonPath = $"{dataPath}/{path}.globaldata";
                if (!File.Exists(jsonPath)) throw new ArgumentException($"global data \"{path}\" not found");

                DeleteFile(jsonPath);
            }
            else
            {
                var directory = $"{dataPath}/{path}";
                foreach (var file in Directory.GetFiles(directory, "*.globaldata", SearchOption.AllDirectories))
                {
                    DeleteFile(file);
                }
            }
        }

        /// <summary>
        /// erases key from .globaldata file
        /// </summary>
        /// <param name="path">local path of .globaldata file</param>
        /// <param name="key">name of json property</param>
        /// <exception cref="ArgumentException"></exception>
        public static void Erase(string path, string key)
        {
            if (string.IsNullOrEmpty(path)) throw new ArgumentException("path cannot be null or empty");
            if (path.EndsWith("/")) throw new ArgumentException("path must specify a file name");

            var jsonPath = $"{dataPath}/{path}.globaldata";
            if (!File.Exists(jsonPath)) throw new ArgumentException($"global data \"{path}\" not found");

            var json = File.ReadAllText(jsonPath);
            if (string.IsNullOrEmpty(json)) throw new ArgumentException($"globaldata at \"{path}\" is empty");

            var isEncrypted = json[0] != '{';
            if (isEncrypted) json = EncryptOrDecrypt(json);

            var jsonObject = JObject.Parse(json);
            jsonObject.Remove(key);

            var data = isEncrypted ? EncryptOrDecrypt(jsonObject.ToString()) : jsonObject.ToString();
            File.WriteAllText(jsonPath, data);
        }

        /// <summary>
        /// saves value to .globaldata file
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path">local path of .globaldata file</param>
        /// <param name="key">name of json property</param>
        /// <param name="value">value to save</param>
        /// <param name="encrypt">should .globaldata file be encrypted</param>
        /// <exception cref="ArgumentException"></exception>
        // представь
        // зима
        // ты гуляешь в девочкой которая тебе нравится
        // и тут ты видишь
        // прям пиздец какой ахуенный лед
        // ну и просто не можешь пройти мимо
        // решаешь проехать на нем
        // и падаешь нахуй
        // в сугроб
        // лежишь блять
        // настолько в снегу
        // что даже если бы был черножопым то стал бы человеком
        // ну и она думает
        // ну а хули
        // пойду тоже в сугроб ебнусь
        // она падает
        // И ТЫ РЕЗКО ВСТАЕШЬ
        // ХВАТАЕШЬ АРМАТУРУ
        // И ХУЯРИШЬ
        // потом она сдохла
        public static void Save<T>(string path, string key, T value, bool encrypt = true)
        {
            if (string.IsNullOrEmpty(path)) throw new ArgumentException("path cannot be null or empty");
            if (path.EndsWith("/")) throw new ArgumentException("path must specify a file name");

            var jsonPath = $"{dataPath}/{path}.globaldata";
            var json = File.Exists(jsonPath) ? File.ReadAllText(jsonPath) : "{}";
            if (string.IsNullOrEmpty(json)) json = "{}";
            else if (json[0] != '{') json = EncryptOrDecrypt(json);

            var jsonObject = JObject.Parse(json);
            jsonObject[key] = JToken.FromObject(value);

            var data = encrypt ? EncryptOrDecrypt(jsonObject.ToString()) : jsonObject.ToString();

            Directory.CreateDirectory(Path.GetDirectoryName(jsonPath));
            File.WriteAllText(jsonPath, data);
        }

        /// <summary>
        /// loads value from .globaldata file
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path">local path of .globaldata file</param>
        /// <param name="key">name of json property</param>
        /// <returns>value of json property</returns>
        /// <exception cref="ArgumentException"></exception>
        public static T Load<T>(string path, string key)
        {
            if (string.IsNullOrEmpty(path)) throw new ArgumentException("path cannot be null or empty");
            if (path.EndsWith("/")) throw new ArgumentException("path must specify a file name");

            var jsonPath = $"{dataPath}/{path}.globaldata";
            if (!File.Exists(jsonPath)) throw new ArgumentException($"global data \"{path}\" not found");

            var json = File.ReadAllText(jsonPath);
            if (string.IsNullOrEmpty(json)) throw new ArgumentException($"globaldata at \"{path}\" is empty");
            else if (json[0] != '{') json = EncryptOrDecrypt(json);

            var jsonObject = JObject.Parse(json);
            if (!jsonObject.ContainsKey(key))
            {
                throw new ArgumentException($"cant load \"{key}\" at \"{path}\" because it does not exist");
            }

            try
            {
                var obj = jsonObject[key].ToObject<T>();
                return obj;
            }
            catch
            {
                throw new ArgumentException($"cant load {jsonObject[key].Type} \"{key}\" as {typeof(T).GenericToString()}");
            }
        }

        /// <summary>
        /// loads value from .globaldata file
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path">local path of .globaldata file</param>
        /// <param name="key">name of json property</param>
        /// <param name="defaultValue">value to return if property was not found</param>
        /// <returns>value of json property</returns>
        /// <exception cref="ArgumentException"></exception>
        public static T Load<T>(string path, string key, T defaultValue)
        {
            if (string.IsNullOrEmpty(path)) throw new ArgumentException("path cannot be null or empty");
            if (path.EndsWith("/")) throw new ArgumentException("path must specify a file name");

            var jsonPath = $"{dataPath}/{path}.globaldata";
            if (!File.Exists(jsonPath)) return defaultValue;

            var json = File.ReadAllText(jsonPath);
            if (string.IsNullOrEmpty(json)) throw new ArgumentException($"globaldata at \"{path}\" is empty");
            else if (json[0] != '{') json = EncryptOrDecrypt(json);

            var jsonObject = JObject.Parse(json);
            if (!jsonObject.ContainsKey(key))
            {
                return defaultValue;
            }

            try
            {
                var obj = jsonObject[key].ToObject<T>();
                return obj;
            }
            catch
            {
                throw new ArgumentException($"cant load {jsonObject[key].Type} \"{key}\" as {typeof(T).GenericToString()}");
            }
        }

        private static string EncryptOrDecrypt(string data)
        {
            var output = "";
            for (int i = 0; i < data.Length; i++)
            {
                output += (char)(data[i] ^ _encryptionKey[i % _encryptionKey.Length]);
            }

            return output;
        }

        private static readonly string _encryptionKey = @"Крч, есть один такой тип
кафиф его звать, или просто лошок.
Вечно сука сонный, так и хочет спать! Как ни сходишь с ним на прогулку, так сразу дрыхнет. А если его разбудить..... Кразу пропишет тебе в ебало гравием, и ты
уснешь вместе с ним. Но, даже не смотря на это он адекват, не курит, не пьет, но дрочит, ибо у него никогда не было девушки, впрочем дефолтная ситуация.
Как сам он рассказывал у него 16 мм (упс, имел ввиду сантиметров), но чет больно пиздит, даже если и дрочит, то сомневаюсь, что он смог столько надрочить.
Перейдем к истории:
Как-то кафиф как обычно спал, но тут его разбудил ебливый будильник, кафиф конечно уебал по будильнику знатно, что тот нахуй разлетелся на части. Кайф
шел не долго, сразу прозвенел телефон. кафиф уже был готов наорать в трубку, как тут он услышал:
?: Привет, кафиф!
Это был голос его подруги Партура:
К: Ну.... Привет...
П: Клегка усмехнулась. Как насчёт прогуляться?
К: Хм, почему-бы и нет, где гулять будем? Как всегда в Ктарр Парке?
П: Честно незнаю, в Ктарр Парке уже надоело, там очень часто гуляем... А хотя, других вариантов нет, в 2 часа сможем?
К: Думаю да...
П: Ну, тогда давай, до встречи. Немного хихикнула
Насчёт смеха, кафиф не придал внимания, он думал, что это нормально, над ним прост угарают;
14:00
кафиф уже ждал партура в Ктарр Парке, достаточно долго, но чего тут ожидать, это-же девушка, хех.
Как тут, кафиф ударило что-то, слабо, но неприятно. Обернувшись, тот увидел 3 клона тары, сначала кафиф удивился, но потом пришел в себя. Чуть позже,
клоны исчезли, а рядом появилась Партур, слегка посмеиваясь:
П: Похоже, что всё-таки клоны не ошиблись, ударив тебя, хи-хи
К: Ага... Это уж точно... Ну, что.... Как обычно?
П: Как обычно, а, слушай... Может после прогулки зайдешь ко мне? Просто одной мне бывает скучно, хоть компанию составишь....
кафиф сначала думал, а стоит-ли? Может она врёт? Ведь Партур всегда подкалывала его:
К: Это очередной рофл?
П: Можешь думать, как хочешь, но скажу так: это не шутка, я на полном
серьёзе.
кафиф слегка приохуел, что его лошка, куда-то приглашают, помимо прогулки:
К: Э.... Ну.... Я даже не знаю заикался
П: Хи-хи, похоже ты действительно удивлен.
К: Не то слово... Ладно, почему-бы и нет...
П: Кпасибо!
К такими словами, Партур сильно обняла кафиф, что тот немного охуел, и даже покраснел. Далее они продолжили прогулку:
Говоря о Партуру: Кама она охуенная девчонка, со стилем она не заморачивается, ходит в одном и том-же, фигура стройная, без наклона на жир. Хоть она лицо
и скрывает, но по глазам видно она достаточно дерзкая, но уверенная. Однако самое охуенное это её грудь. Грудь 3-го размера сильно заводит, даже лошка
кафиф. Говоря о кафиф: тот случайно засмотрелся в ее грудь, что у него кажись встал. Пот сразу отвернулся, чтобы его не отшили.
16:20
Вот, кафиф и Партур уже нагулялись, по пути решили сгонять в магаз за пивцом. Хоть кафиф не пил, но решил попробовать, хуле нет?
Купили Жигулёвского, литров 2, и айда прямиком к Партуру на хату:
П: Ну, как прогулка задалась?
К: Это было охуенно, просто пиздато я-бы даже сказал!
П: Клегка просмеялась Оно видно)
16:25
-
И так, лошок и Партур вошли к ней домой: А ведь квартирка-то не хуевая, никакая это не хрущевка, а вполне адекватная квартира, с уклоном на модерн. Далее,
кафиф войдя на кухню, приметив партура, которая уже наливала бухла:
- Ну, хули встал-то? Пойдем, откинемся, сказала Партур
- А чё? Погнали нахуй, ответил кафиф.
Вот и начали оба лошка пить...
16:50
Кпустя какое-то время, оба уже были в полном пиздеце. Зато как они начали говорить по душам....
Затем Партуру чет херово стало, пошла блевать, а кафиф просто зырил на её зад, когда та уходила. Бля, а ведь после прогулки-то упал...
Однако, сразу же после просмотра на пятую точку встал, да и вроде даже больше стал. Ктояк выпирал из штанов, делая небольшую ""кочку"" на штанах.
17:00
Вот, наконец-то, Партур облегчилась, а лошок кафиф так и сидел на стуле, со стояком. Па позвала его к себе в комнату, мол, хули на кухне сидеть. В к
омнате они оба лежали на кровати, кафиф лег на спину, а Партур улеглась на него сверху. Они так лежали около 10 минут, пока стояк кафиф не дал знать о себе.
Партур почуяла, что между ее задом что-то твердое. Она сначала не подавала признаков, но когда уже она не могла это терпеть, она покраснела, и начала
надрачивать задом член кафиф. Пот почуял кайф, что даже голову опрокинул назад. Чуть позже, Партур слезла с него и начала потихоньку снимать с него штаны, попутно мастурбируя себе пизду. 
Княв с Лошка штаны, она охуела от размера:
- Надрочил себе небось, подумала Партур.
Она начала дрочить его, а затем.... Как резко возьмёт его в рот, начав отсасывать. Черт возьми, а у нее это получалось, что кафиф уже лежал в кайфе. Уже спустя
2 минуты, кафиф кончил ей в рот, последовавший стоном от него. Партур вытащила член из-за рта, проглотив сперму. Далее, Партур села на член кафиф,
причем без смазки, ведь пизда Партура уже текла ручьем. Последовал милый, но громкий стон. Она прыгала на члене кафифа, и получала кайф. кафиф также
получал кайф от этого:
- М-гхм...., стонала Партур., м-м.....
Чуть позже, Поза надоела, и продолжили в missionary. кафиф ебал пизду Партура как мог: Глубоко, быстро, сильнее. Это сопровождалось громкими стонами
Партура, которая так кайфовала от ебли, что только и просила сильнее. кафиф почуял, что уже близко, и сначала остановился, а затем... Как начал ее ебать,
наверное со скоростью света:
-А-А-Аг-мх....~~~ А-а-а-гх.... мм-ммм.....ммм-гх...мг-мм-ах...... стонала Партур.
Она-же умоляла кончить внутрь, придерживая лошка к себе, к ее груди, которую кафиф лапал, как зверь. В итоге лошок кончает в нее, от чего тот чуть-ли не
вырубился. Партур-же, когда лошок упал, начала снова ублажать его член, несколько раз. За это время лошок кончил ещё 2 раза, прямиком в ее ротик, ну, а
лошок только и кайфовал и кончал, пока и вовсе не отрубился. Чуть позже Партур закончила ублажать член кафифа, сама она уже была в сперме, слизывая ее,
затем легла рядом с кафиф, иногда подрачивая ему ручкой, от чего тот успел ещё раз кончить.
Вот наступило утро, кафиф проснулся с счастьем на лице, в постели у Партура, а та уже давно проснулась. Она поцеловала кафифа, сказав:
- Это была охуенная ночь, я никогда так не развлекалась, как с тобой.... . Слушай, может как-нибудь заскочишь ко мне вечерком? Я хочу повторить...
Чуть позже, кафиф ушел с ее квартиры, задумываясь насколько эта была охуенная ночь..."; // global data is fully encrypted
    }
}