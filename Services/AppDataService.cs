using Microsoft.EntityFrameworkCore;
using QazaqQuest.Data;
using QazaqQuest.Models;

namespace QazaqQuest.Services;

public class AppDataService
{
    private readonly AppDbContext _dbContext;

    public AppDataService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public void EnsureSeedData()
    {
        var existingTitles = _dbContext.Quests.Select(q => q.Title).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var seededQuests = SeedQuests()
            .Where(q => !existingTitles.Contains(q.Title))
            .ToList();

        if (!seededQuests.Any())
            return;

        foreach (var quest in seededQuests)
        {
            var originalPoints = quest.Points.ToList();
            quest.Points = new List<QuestPoint>();

            foreach (var point in originalPoints)
            {
                var location = new QuestLocation
                {
                    Name = point.Name,
                    Address = point.Name,
                    Latitude = point.Latitude,
                    Longitude = point.Longitude,
                    RadiusMeters = point.RadiusMeters
                };

                point.Location = location;
                point.QuestLocationId = null;
                quest.Locations.Add(location);
                quest.Points.Add(point);
            }
        }

        _dbContext.Quests.AddRange(seededQuests);
        _dbContext.SaveChanges();
    }

    public List<Quest> GetQuests() => _dbContext.Quests
        .Include(q => q.Points).ThenInclude(p => p.Location)
        .Include(q => q.Rewards)
        .Include(q => q.Locations)
        .OrderBy(q => q.Title)
        .ToList();

    public Quest? GetQuestById(int id) => _dbContext.Quests
        .Include(q => q.Points).ThenInclude(p => p.Location)
        .Include(q => q.Rewards)
        .Include(q => q.Locations)
        .FirstOrDefault(q => q.Id == id);

    public List<string> GetCities() => _dbContext.Quests.Select(q => q.City).Distinct().OrderBy(c => c).ToList();
    public List<string> GetDifficulties() => _dbContext.Quests.Select(q => q.Difficulty).Distinct().OrderBy(c => c).ToList();
    public List<string> GetTypes() => _dbContext.Quests.Select(q => q.Type).Distinct().OrderBy(c => c).ToList();

    public List<Quest> FilterQuests(string? city, string? difficulty, string? type, string? search)
    {
        IQueryable<Quest> query = _dbContext.Quests
            .Include(q => q.Points).ThenInclude(p => p.Location)
            .Include(q => q.Rewards)
            .Include(q => q.Locations);

        if (!string.IsNullOrWhiteSpace(city))
            query = query.Where(q => q.City.ToLower() == city.ToLower());
        if (!string.IsNullOrWhiteSpace(difficulty))
            query = query.Where(q => q.Difficulty.ToLower() == difficulty.ToLower());
        if (!string.IsNullOrWhiteSpace(type))
            query = query.Where(q => q.Type.ToLower() == type.ToLower());
        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalized = search.Trim().ToLower();
            query = query.Where(q =>
                q.Title.ToLower().Contains(normalized) ||
                q.Description.ToLower().Contains(normalized) ||
                q.City.ToLower().Contains(normalized) ||
                q.Category.ToLower().Contains(normalized) ||
                q.Audience.ToLower().Contains(normalized));
        }

        return query.OrderBy(q => q.Title).ToList();
    }

    public int GetTotalRewardPoints(IEnumerable<Quest> quests) => quests.Sum(q => q.Rewards.Sum(r => r.Points));
    public int GetTotalRoutePoints() => _dbContext.QuestPoints.Count();

    public double CalculateDistanceMeters(double lat1, double lon1, double lat2, double lon2)
    {
        const double earthRadius = 6371000;
        var dLat = DegreesToRadians(lat2 - lat1);
        var dLon = DegreesToRadians(lon2 - lon1);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(DegreesToRadians(lat1)) * Math.Cos(DegreesToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return earthRadius * c;
    }

    private static double DegreesToRadians(double angle) => angle * Math.PI / 180.0;

private static List<Quest> SeedQuests()
    {
        return new List<Quest>
        {
            new()
            {
                Id = 1,
                Title = "Тайны Бәйтерека",
                Description = "Квест по символам новой столицы: панорамы, легенды о дереве жизни, архитектурные детали и точные наблюдения.",
                City = "Астана", Difficulty = "Средний", Type = "Бесплатный", Price = 0, Duration = "55–75 минут", RouteLength = "2.8 км",
                Category = "Архитектура и история", Audience = "Туристы, пары, друзья", ImageUrl = "/photo/quest-bayterek.svg",
                CoverStyle = "linear-gradient(135deg, #0f766e 0%, #38bdf8 100%)", Icon = "🌇", Partner = "Sky Coffee", Bonus = "Напиток 1+1 после финиша", IsFeatured = true, ExperienceReward = 130, CoinsReward = 35,
                Points = new List<QuestPoint>
                {
                    new() { Id = 101, Order = 1, Name = "Бәйтерек", TaskType = "Текстовый вопрос", Task = "Подойди к монументу и введи кодовое слово: өмір.", Answer = "өмір", Hint = "Слово связано с идеей дерева жизни.", Latitude = 51.1282, Longitude = 71.4304, RadiusMeters = 180 },
                    new() { Id = 102, Order = 2, Name = "Бульвар Нуржол", TaskType = "Выбор варианта", Task = "Что лучше всего описывает эту локацию?", Answer = "пешеходный бульвар", Hint = "Не вокзал и не рынок.", Options = new List<string>{"рынок","пешеходный бульвар","стадион"}, Latitude = 51.1277, Longitude = 71.4288, RadiusMeters = 170 },
                    new() { Id = 103, Order = 3, Name = "Поющий фонтан", TaskType = "Фото-задание", Task = "Сделай фото локации и введи подтверждающее слово: су.", Answer = "су", Hint = "По-казахски это вода.", Latitude = 51.1272, Longitude = 71.4267, RadiusMeters = 190 },
                    new() { Id = 104, Order = 4, Name = "Финиш у Хан Шатыра", TaskType = "Текстовый вопрос", Task = "Какая форма у главного здания впереди? Введи слово: шатер.", Answer = "шатер", Hint = "Это большая прозрачная палатка города.", Latitude = 51.1326, Longitude = 71.4035, RadiusMeters = 220 }
                },
                Rewards = new List<Reward>
                {
                    new() { Id = 1001, Title = "Хранитель панорамы", Description = "Открыл главные виды центра Астаны.", Points = 120 },
                    new() { Id = 1002, Title = "Глаз столицы", Description = "Прошёл маршрут без пропуска точек.", Points = 35 }
                }
            },
            new()
            {
                Id = 2,
                Title = "Jastar Code Run",
                Description = "Быстрый молодёжный маршрут у Jastar: уличная энергия, короткие задания, GPS-подтверждение и командный темп.",
                City = "Астана", Difficulty = "Лёгкий", Type = "Бесплатный", Price = 0, Duration = "35–50 минут", RouteLength = "1.6 км",
                Category = "Молодёжный urban-квест", Audience = "Студенты, друзья, подростки", ImageUrl = "/photo/quest-jastar.svg",
                CoverStyle = "linear-gradient(135deg, #7c3aed 0%, #06b6d4 100%)", Icon = "🧭", Partner = "Jastar Space", Bonus = "Бонусный бейдж и промокод", IsFeatured = true, ExperienceReward = 100, CoinsReward = 20,
                Points = new List<QuestPoint>
                {
                    new() { Id = 201, Order = 1, Name = "Jastar Astana", TaskType = "Текстовый вопрос", Task = "Доберись до стартовой точки и введи слово: жастар.", Answer = "жастар", Hint = "Совпадает с названием локации.", Latitude = 51.170104, Longitude = 71.427882, RadiusMeters = 140 },
                    new() { Id = 202, Order = 2, Name = "Urban spot рядом", TaskType = "Выбор варианта", Task = "Какая атмосфера подходит месту лучше всего?", Answer = "молодёжное пространство", Hint = "Ответ про идеи, общение и активность.", Options = new List<string>{"промзона","молодёжное пространство","закрытый склад"}, Latitude = 51.169450, Longitude = 71.429050, RadiusMeters = 150 },
                    new() { Id = 203, Order = 3, Name = "Challenge wall", TaskType = "Фото-задание", Task = "Сделай фото точки и введи слово: серпін.", Answer = "серпін", Hint = "Это слово про импульс и движение вперёд.", Latitude = 51.171050, Longitude = 71.426900, RadiusMeters = 150 },
                    new() { Id = 204, Order = 4, Name = "Финишный круг", TaskType = "Текстовый вопрос", Task = "Финальный код маршрута: энергия. Введи это слово.", Answer = "энергия", Hint = "Оставайся в атмосфере квеста.", Latitude = 51.170600, Longitude = 71.428200, RadiusMeters = 150 }
                },
                Rewards = new List<Reward>
                {
                    new() { Id = 2001, Title = "Jastar Explorer", Description = "Завершил молодёжный маршрут.", Points = 90 },
                    new() { Id = 2002, Title = "Urban Pulse", Description = "Прошёл быстрый городской челлендж.", Points = 25 }
                }
            },
            new()
            {
                Id = 3,
                Title = "EXPO: Энергия будущего",
                Description = "Маршрут по району EXPO с заданиями про форму, свет, устойчивое будущее и фото-ориентиры.",
                City = "Астана", Difficulty = "Средний", Type = "Платный", Price = 2490, Duration = "65–85 минут", RouteLength = "3.1 км",
                Category = "Технологии и архитектура", Audience = "Студенты, туристы, команды", ImageUrl = "/photo/quest-expo.svg",
                CoverStyle = "linear-gradient(135deg, #2563eb 0%, #22c55e 100%)", Icon = "⚡", Partner = "Expo Hub", Bonus = "Скидка на кофе в павильоне", IsTimed = true, TimeLimitMinutes = 80, IsFeatured = true, ExperienceReward = 160, CoinsReward = 45,
                Points = new List<QuestPoint>
                {
                    new() { Id = 301, Order = 1, Name = "Сфера Нур Алем", TaskType = "Текстовый вопрос", Task = "Посмотри на главное здание и введи слово: сфера.", Answer = "сфера", Hint = "Форма идеально круглая.", Latitude = 51.0907, Longitude = 71.4181, RadiusMeters = 180 },
                    new() { Id = 302, Order = 2, Name = "Площадь EXPO", TaskType = "Выбор варианта", Task = "Какой вайб у пространства?", Answer = "современный технологичный квартал", Hint = "Ответ длиннее остальных и ближе к теме будущего.", Options = new List<string>{"старый рынок","современный технологичный квартал","частный сектор"}, Latitude = 51.0914, Longitude = 71.4165, RadiusMeters = 170 },
                    new() { Id = 303, Order = 3, Name = "Аллея света", TaskType = "Фото-задание", Task = "Сделай фото маршрута и введи слово: жарық.", Answer = "жарық", Hint = "Это свет по-казахски.", Latitude = 51.0922, Longitude = 71.4148, RadiusMeters = 170 },
                    new() { Id = 304, Order = 4, Name = "Финишный вопрос", TaskType = "Текстовый вопрос", Task = "Главная тема маршрута — энергия. Введи слово: болашақ.", Answer = "болашақ", Hint = "Это будущее по-казахски.", Latitude = 51.0910, Longitude = 71.4196, RadiusMeters = 180 }
                },
                Rewards = new List<Reward>
                {
                    new() { Id = 3001, Title = "Energy Hunter", Description = "Прошёл технологичный маршрут у EXPO.", Points = 140 },
                    new() { Id = 3002, Title = "Future Lens", Description = "Собрал все фото-подтверждения.", Points = 40 }
                }
            },
            new()
            {
                Id = 4,
                Title = "Ночной Нуржол",
                Description = "Вечерний маршрут по огням бульвара: видовые точки, иллюминация, архитектурные подсказки и атмосферные задания.",
                City = "Астана", Difficulty = "Средний", Type = "Платный", Price = 1990, Duration = "50–70 минут", RouteLength = "2.4 км",
                Category = "Вечерний city walk", Audience = "Пары, друзья, блогеры", ImageUrl = "/photo/quest-nurzhol.svg",
                CoverStyle = "linear-gradient(135deg, #1d4ed8 0%, #a855f7 100%)", Icon = "🌙", Partner = "Night Roast", Bonus = "Скидка на десерт после 19:00",
                Points = new List<QuestPoint>
                {
                    new() { Id = 401, Order = 1, Name = "Старт у Нуржола", TaskType = "Текстовый вопрос", Task = "Найди начало вечернего маршрута и введи: түн.", Answer = "түн", Hint = "Это ночь по-казахски.", Latitude = 51.1279, Longitude = 71.4281, RadiusMeters = 160 },
                    new() { Id = 402, Order = 2, Name = "Зона огней", TaskType = "Выбор варианта", Task = "Что здесь чувствуется сильнее всего?", Answer = "ритм большого города", Hint = "Выбирай вариант про темп столицы.", Options = new List<string>{"тишина аула","ритм большого города","лесная тропа"}, Latitude = 51.1288, Longitude = 71.4256, RadiusMeters = 160 },
                    new() { Id = 403, Order = 3, Name = "Световой кадр", TaskType = "Фото-задание", Task = "Сделай фото подсветки и введи слово: жарық.", Answer = "жарық", Hint = "Слово уже встречалось, но здесь оно про огни.", Latitude = 51.1298, Longitude = 71.4228, RadiusMeters = 170 },
                    new() { Id = 404, Order = 4, Name = "Финиш у панорамы", TaskType = "Текстовый вопрос", Task = "Финальный код — skyline. Введи слово: skyline.", Answer = "skyline", Hint = "Английское слово про линию города.", Latitude = 51.1307, Longitude = 71.4204, RadiusMeters = 180 }
                },
                Rewards = new List<Reward>
                {
                    new() { Id = 4001, Title = "Night Walker", Description = "Поймал атмосферу вечерней Астаны.", Points = 110 },
                    new() { Id = 4002, Title = "Light Collector", Description = "Собрал все световые точки.", Points = 30 }
                }
            },
            new()
            {
                Id = 5,
                Title = "Сердце столицы: Ақорда и набережная",
                Description = "Маршрут о символах власти, просторных площадях и спокойном ритме у воды. Подходит для вдумчивой прогулки.",
                City = "Астана", Difficulty = "Сложный", Type = "Бесплатный", Price = 0, Duration = "80–100 минут", RouteLength = "3.6 км",
                Category = "История и символы", Audience = "Студенты, экскурсионные группы", ImageUrl = "/photo/quest-akorda.svg",
                CoverStyle = "linear-gradient(135deg, #0f172a 0%, #0ea5e9 100%)", Icon = "🏛️", Partner = "River Point", Bonus = "Памятный цифровой сертификат",
                Points = new List<QuestPoint>
                {
                    new() { Id = 501, Order = 1, Name = "Ақорда", TaskType = "Текстовый вопрос", Task = "Доберись до обзорной точки и введи слово: орда.", Answer = "орда", Hint = "Часть названия резиденции.", Latitude = 51.1254, Longitude = 71.4342, RadiusMeters = 180 },
                    new() { Id = 502, Order = 2, Name = "Площадь Независимости", TaskType = "Выбор варианта", Task = "Какая ассоциация ближе всего?", Answer = "торжественное пространство", Hint = "Ответ про масштаб и церемониальность.", Options = new List<string>{"торжественное пространство","подземный паркинг","рынок выходного дня"}, Latitude = 51.1242, Longitude = 71.4365, RadiusMeters = 180 },
                    new() { Id = 503, Order = 3, Name = "Набережная Есиля", TaskType = "Фото-задание", Task = "Сделай фото у воды и введи: өзен.", Answer = "өзен", Hint = "Это река по-казахски.", Latitude = 51.1233, Longitude = 71.4398, RadiusMeters = 190 },
                    new() { Id = 504, Order = 4, Name = "Финишный код", TaskType = "Текстовый вопрос", Task = "Заверши маршрут словом: астана.", Answer = "астана", Hint = "Название города.", Latitude = 51.1224, Longitude = 71.4422, RadiusMeters = 190 }
                },
                Rewards = new List<Reward>
                {
                    new() { Id = 5001, Title = "Capital Heart", Description = "Прошёл маршрут по символам государства.", Points = 150 },
                    new() { Id = 5002, Title = "Calm River", Description = "Завершил прогулку вдоль Есиля.", Points = 35 }
                }
            },
            new()
            {
                Id = 6,
                Title = "Ботанический код",
                Description = "Зелёный маршрут по Ботаническому саду: природа, лёгкий темп, эко-задания и приятные паузы для фото.",
                City = "Астана", Difficulty = "Лёгкий", Type = "Бесплатный", Price = 0, Duration = "45–60 минут", RouteLength = "2.1 км",
                Category = "Эко-прогулка", Audience = "Семьи, пары, прогулки днём", ImageUrl = "/photo/quest-botanical.svg",
                CoverStyle = "linear-gradient(135deg, #16a34a 0%, #0ea5e9 100%)", Icon = "🌿", Partner = "Green Cup", Bonus = "Стикерпак за прохождение",
                Points = new List<QuestPoint>
                {
                    new() { Id = 601, Order = 1, Name = "Вход в сад", TaskType = "Текстовый вопрос", Task = "Старт маршрута — введи слово: бақ.", Answer = "бақ", Hint = "Так по-казахски звучит сад.", Latitude = 51.1082, Longitude = 71.4012, RadiusMeters = 160 },
                    new() { Id = 602, Order = 2, Name = "Аллея", TaskType = "Выбор варианта", Task = "Какой режим подходит прогулке лучше всего?", Answer = "спокойный ритм", Hint = "Не спешка и не хаос.", Options = new List<string>{"шумный марафон","спокойный ритм","авто-рейс"}, Latitude = 51.1094, Longitude = 71.4029, RadiusMeters = 160 },
                    new() { Id = 603, Order = 3, Name = "Фототочка", TaskType = "Фото-задание", Task = "Сделай зелёный кадр и введи слово: жапырақ.", Answer = "жапырақ", Hint = "Это лист.", Latitude = 51.1108, Longitude = 71.4046, RadiusMeters = 160 },
                    new() { Id = 604, Order = 4, Name = "Финиш", TaskType = "Текстовый вопрос", Task = "Финальный код: тыныштық.", Answer = "тыныштық", Hint = "Это спокойствие.", Latitude = 51.1116, Longitude = 71.4063, RadiusMeters = 170 }
                },
                Rewards = new List<Reward>
                {
                    new() { Id = 6001, Title = "Green Walker", Description = "Прошёл природный маршрут по Астане.", Points = 95 },
                    new() { Id = 6002, Title = "Eco Frame", Description = "Собрал все зелёные кадры.", Points = 25 }
                }
            },
            new()
            {
                Id = 7,
                Title = "Street Art Astana",
                Description = "Фото-квест по ярким городским деталям: муралы, фактуры, городские цвета и внимательность к мелочам.",
                City = "Астана", Difficulty = "Средний", Type = "Платный", Price = 1490, Duration = "55–70 минут", RouteLength = "2.2 км",
                Category = "Фото и креатив", Audience = "Блогеры, друзья, творческие команды", ImageUrl = "/photo/quest-art.svg",
                CoverStyle = "linear-gradient(135deg, #db2777 0%, #8b5cf6 100%)", Icon = "🎨", Partner = "Art Coffee", Bonus = "Скидка на печать фото",
                Points = new List<QuestPoint>
                {
                    new() { Id = 701, Order = 1, Name = "Старт у арт-точки", TaskType = "Текстовый вопрос", Task = "Введи код старта: color.", Answer = "color", Hint = "Английское слово про цвет.", Latitude = 51.1361, Longitude = 71.4098, RadiusMeters = 160 },
                    new() { Id = 702, Order = 2, Name = "Мурал", TaskType = "Выбор варианта", Task = "Что делает эту локацию особенной?", Answer = "крупный городской рисунок", Hint = "Речь про настенное изображение.", Options = new List<string>{"подземный переход","крупный городской рисунок","спортивный зал"}, Latitude = 51.1373, Longitude = 71.4112, RadiusMeters = 160 },
                    new() { Id = 703, Order = 3, Name = "Фото-задание", TaskType = "Фото-задание", Task = "Сделай кадр с цветом и введи: реңк.", Answer = "реңк", Hint = "Это оттенок.", Latitude = 51.1380, Longitude = 71.4128, RadiusMeters = 170 },
                    new() { Id = 704, Order = 4, Name = "Финиш", TaskType = "Текстовый вопрос", Task = "Финальное слово: urban.", Answer = "urban", Hint = "Стиль города.", Latitude = 51.1390, Longitude = 71.4140, RadiusMeters = 170 }
                },
                Rewards = new List<Reward>
                {
                    new() { Id = 7001, Title = "Art Scanner", Description = "Нашёл все визуальные акценты маршрута.", Points = 105 },
                    new() { Id = 7002, Title = "Creative Eye", Description = "Собрал фотоколлекцию по пути.", Points = 30 }
                }
            },

            new()
            {
                Id = 9,
                Title = "Master Coffee",
                Description = "Кофейный мини-квест у Master Coffee Express: найди нужную точку, осмотрись внутри, узнай имя бариста и введи правильный ответ.",
                City = "Астана", Difficulty = "Лёгкий", Type = "Бесплатный", Price = 0, Duration = "20–30 минут", RouteLength = "0.4 км",
                Category = "Кофейный city-квест", Audience = "Друзья, пары, студенты", ImageUrl = "/photo/food-quest.jpg",
                CoverStyle = "linear-gradient(135deg, #6f4e37 0%, #d4a373 100%)", Icon = "☕", Partner = "Master Coffee Express", Bonus = "Бейдж Coffee Hunter и идея для сторис", IsHidden = true, UnlockLevel = 2, ExperienceReward = 80, CoinsReward = 30,
                Points = new List<QuestPoint>
                {
                    new() { Id = 901, Order = 1, Name = "Master Coffee Express", TaskType = "Текстовый вопрос", Task = "Приди к кофейне Master Coffee Express и подтверди старт. Введи кодовое слово: кофе.", Answer = "кофе", Hint = "Самое очевидное слово для старта кофейного маршрута.", Latitude = 51.124485, Longitude = 71.417105, RadiusMeters = 150 },
                    new() { Id = 902, Order = 2, Name = "Зал кофейни", TaskType = "Выбор варианта", Task = "Какое настроение лучше всего подходит этой точке?", Answer = "уютная городская кофейня", Hint = "Не офис и не спортивный зал.", Options = new List<string>{"шумный стадион","уютная городская кофейня","автомойка"}, Latitude = 51.124485, Longitude = 71.417105, RadiusMeters = 150 },
                    new() { Id = 903, Order = 3, Name = "Бариста-чекин", TaskType = "Текстовый вопрос", Task = "Узнай имя бариста и введи его в форму.", Answer = "руслан", Hint = "Имя начинается на Р.", Latitude = 51.124485, Longitude = 71.417105, RadiusMeters = 150 },
                    new() { Id = 904, Order = 4, Name = "Финиш у стойки", TaskType = "Фото-задание", Task = "Сделай фото напитка или стойки и введи финальное слово: арабика.", Answer = "арабика", Hint = "Популярный вид кофейного зерна.", Latitude = 51.124485, Longitude = 71.417105, RadiusMeters = 150 }
                },
                Rewards = new List<Reward>
                {
                    new() { Id = 9001, Title = "Coffee Hunter", Description = "Нашёл кофейную точку и прошёл мини-маршрут до конца.", Points = 70 },
                    new() { Id = 9002, Title = "Знаю бариста", Description = "Успешно прошёл задание с именем бариста Руслан.", Points = 20 }
                }
            },
            new()
            {
                Id = 10,
                Title = "Шифр старого города",
                Description = "Хардкорный маршрут для тех, кто любит не просто гулять, а ломать голову: шифр Цезаря, анаграммы, акростихи и многошаговые подсказки.",
                City = "Астана", Difficulty = "Сложный", Type = "Платный", Price = 3490, Duration = "80–105 минут", RouteLength = "3.4 км",
                Category = "Шифры и логика", Audience = "Опытные игроки, пары, команды", ImageUrl = "/photo/quest-expo.svg",
                CoverStyle = "linear-gradient(135deg, #111827 0%, #4f46e5 100%)", Icon = "🧩", Partner = "Puzzle Point", Bonus = "Секретный бейдж Cipher Master", IsTimed = true, TimeLimitMinutes = 95, UnlockLevel = 2, ExperienceReward = 220, CoinsReward = 70,
                Points = new List<QuestPoint>
                {
                    new() { Id = 1001, Order = 1, Name = "Стартовый обелиск", TaskType = "Шифр Цезаря", Task = "На табличке ты видишь слово «ДУВЩБОБ». Это слово зашифровано сдвигом на 1 букву вперёд по русскому алфавиту. Сдвинь каждую букву на 1 назад и введи ответ без пробелов.", Answer = "астана", Hint = "Пример: Б → А, Е → Д. Разгадай всё слово целиком.", Latitude = 51.1321, Longitude = 71.4205, RadiusMeters = 150 },
                    new() { Id = 1002, Order = 2, Name = "Каменная арка", TaskType = "Анаграмма", Task = "Собери слово из букв «К Е Р Т А Й Б». Это символ столицы и одна из самых узнаваемых точек города. Введи ответ слитно.", Answer = "байтерек", Hint = "Семь букв складываются в название монумента.", Latitude = 51.1314, Longitude = 71.4233, RadiusMeters = 150 },
                    new() { Id = 1003, Order = 3, Name = "Переход с колоннами", TaskType = "Акростих", Task = "Возьми первые буквы строк и собери слово:\nБлиже к небу тянется силуэт\nА внизу шумит город\nЙодовый блеск стекла ловит солнце\nТочка маршрута уже рядом\nЕщё шаг — и увидишь подсказку\nРешение всегда в начале строк\nЕдинственный верный ответ введи слитно\nКакое слово получилось?", Answer = "байтерек", Hint = "Смотри только на первые буквы каждой строки.", Latitude = 51.1307, Longitude = 71.4257, RadiusMeters = 160 },
                    new() { Id = 1004, Order = 4, Name = "Финишная галерея", TaskType = "Логическая цепочка", Task = "Финальный код строится так: 1) возьми первое решение, 2) прибавь к нему второе, 3) убери повторяющиеся буквы, оставив только уникальные в порядке первого появления. Из слов «астана» и «байтерек» получится? Введи итоговое слово без пробелов.", Answer = "астнбайрек", Hint = "Иди слева направо: а-с-т-а-н-а-б-а-й-т-е-р-е-к → оставляй только первую встречу каждой буквы.", Latitude = 51.1299, Longitude = 71.4280, RadiusMeters = 170 }
                },
                Rewards = new List<Reward>
                {
                    new() { Id = 10001, Title = "Cipher Master", Description = "Разобрал цепочку шифров и дошёл до конца без сдачи.", Points = 160 },
                    new() { Id = 10002, Title = "Ледяная логика", Description = "Прошёл один из самых сложных маршрутов проекта.", Points = 55 }
                }
            },
            new()
            {
                Id = 11,
                Title = "Операция: Красная нить",
                Description = "Детективный квест по центру города: ребусы, зеркальные слова, числовые закономерности и финальный код из нескольких частей.",
                City = "Астана", Difficulty = "Сложный", Type = "Платный", Price = 3990, Duration = "90–115 минут", RouteLength = "3.8 км",
                Category = "Ребусы и расследование", Audience = "Команды, друзья, любители головоломок", ImageUrl = "/photo/quest-art.svg",
                CoverStyle = "linear-gradient(135deg, #7f1d1d 0%, #1f2937 100%)", Icon = "🕵️", Partner = "Detective Cafe", Bonus = "Бонусный код и роль в сезонном рейтинге", IsCoop = true, IsFeatured = true, UnlockLevel = 3, ExperienceReward = 260, CoinsReward = 90,
                Points = new List<QuestPoint>
                {
                    new() { Id = 1101, Order = 1, Name = "Красный конверт", TaskType = "Ребус", Task = "Разгадай текстовый ребус: «СФЕ + РА - А + А». Какое слово получится? Введи ответ слитно.", Answer = "сфера", Hint = "Из слова ничего не исчезает: ты снова приходишь к форме главного объекта EXPO.", Latitude = 51.0908, Longitude = 71.3980, RadiusMeters = 150 },
                    new() { Id = 1102, Order = 2, Name = "Зеркальная витрина", TaskType = "Зеркальное слово", Task = "На стекле написано «НАРУ». Прочитай слово наоборот и введи ответ.", Answer = "уран", Hint = "Читай справа налево. Ответ связан с энергией и химическим элементом.", Latitude = 51.0920, Longitude = 71.4001, RadiusMeters = 150 },
                    new() { Id = 1103, Order = 3, Name = "Лестница чисел", TaskType = "Числовая закономерность", Task = "Последовательность точек даёт код: 2, 4, 8, 16, ?. Найди следующее число и введи только число.", Answer = "32", Hint = "Каждый следующий элемент в 2 раза больше предыдущего.", Latitude = 51.0934, Longitude = 71.4022, RadiusMeters = 160 },
                    new() { Id = 1104, Order = 4, Name = "Финальная нить", TaskType = "Составной код", Task = "Собери финальный пароль из первых букв трёх предыдущих ответов в том порядке, как ты их решал. Например, если ответы были «дом», «река», «1», код был бы «др1». Введи свой код.", Answer = "су3", Hint = "Используй первые символы: «сфера», «уран», «32». У числа тоже берётся первый символ.", Latitude = 51.0945, Longitude = 71.4040, RadiusMeters = 170 }
                },
                Rewards = new List<Reward>
                {
                    new() { Id = 11001, Title = "Red Thread", Description = "Собрал все улики и свёл расследование к одному коду.", Points = 180 },
                    new() { Id = 11002, Title = "Детектив Астаны", Description = "Завершил кооперативный хард-квест по логике и ребусам.", Points = 60 }
                }
            },

            new()
            {
                Id = 8,
                Title = "Мосты Есиля",
                Description = "Прогулка по набережной и мостам: вода, ветер, обзорные точки и финальный маршрут на закате.",
                City = "Астана", Difficulty = "Средний", Type = "Бесплатный", Price = 0, Duration = "60–80 минут", RouteLength = "3.0 км",
                Category = "Набережная и виды", Audience = "Пары, друзья, туристы", ImageUrl = "/photo/quest-river.svg",
                CoverStyle = "linear-gradient(135deg, #0891b2 0%, #22c55e 100%)", Icon = "🌉", Partner = "River Side", Bonus = "Цифровой бейдж «Мосты Астаны»", IsCoop = true, ExperienceReward = 140, CoinsReward = 40,
                Points = new List<QuestPoint>
                {
                    new() { Id = 801, Order = 1, Name = "Старт у набережной", TaskType = "Текстовый вопрос", Task = "Введи слово старта: есіл.", Answer = "есіл", Hint = "Название реки.", Latitude = 51.1462, Longitude = 71.4210, RadiusMeters = 180 },
                    new() { Id = 802, Order = 2, Name = "Мост", TaskType = "Выбор варианта", Task = "Что соединяет эта точка?", Answer = "берега города", Hint = "Ответ про две стороны реки.", Options = new List<string>{"горы и степь","берега города","старый вокзал и аэропорт"}, Latitude = 51.1474, Longitude = 71.4244, RadiusMeters = 180 },
                    new() { Id = 803, Order = 3, Name = "Видовая точка", TaskType = "Фото-задание", Task = "Сделай фото набережной и введи: самал.", Answer = "самал", Hint = "Это лёгкий ветер.", Latitude = 51.1488, Longitude = 71.4270, RadiusMeters = 180 },
                    new() { Id = 804, Order = 4, Name = "Финишный код", TaskType = "Текстовый вопрос", Task = "Финальное слово: bridge.", Answer = "bridge", Hint = "Английское слово про мост.", Latitude = 51.1499, Longitude = 71.4296, RadiusMeters = 190 }
                },
                Rewards = new List<Reward>
                {
                    new() { Id = 8001, Title = "River Route", Description = "Закрыл маршрут по набережной Есиля.", Points = 115 },
                    new() { Id = 8002, Title = "Wind Walker", Description = "Дошёл до финиша без ошибок.", Points = 25 }
                }
            }
        };
    }
}

