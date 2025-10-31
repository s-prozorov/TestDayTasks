# TestDayTasks - Сервер карты и клиент

Тайловая система карты с геопространственным хранилищем объектов, построенная на MagicOnion (gRPC) и Redis.

## Быстрый старт с Docker (рекомендуется)

Самый простой способ запустить приложение - использовать Docker:

```bash
# Запуск сервера и Redis
docker-compose up --build

# Или запуск в фоновом режиме
docker-compose up -d --build
```

Сервер будет доступен по адресу `http://localhost:5000`

### Запуск с интерактивным клиентом

```bash
# Запуск всех сервисов включая интерактивный клиент
docker-compose --profile with-client up --build

# Или подключение к уже запущенной системе
docker-compose --profile with-client run --rm mapclient
```

### Остановка сервисов

```bash
docker-compose down

# Удалить volumes тоже
docker-compose down -v
```

## Ручная настройка (без Docker)

### Требования

- .NET 8.0 SDK
- Redis сервер (localhost:6379)

### Установка Redis

**macOS (используя Homebrew):**
```bash
brew install redis
brew services start redis
```

**Ubuntu/Debian:**
```bash
sudo apt-get install redis-server
sudo systemctl start redis-server
```

**Windows:**
Скачайте и установите с [Redis for Windows](https://github.com/microsoftarchive/redis/releases)

**Или используйте Docker:**
```bash
docker run -d -p 6379:6379 redis:7-alpine
```

## Ручной запуск

### 1. Соберите решение
```bash
dotnet build
```

### 2. Запустите сервер (Терминал 1)
```bash
cd MapServer
dotnet run
```

Вы должны увидеть:
```
MapServer starting...
Connecting to Redis: localhost:6379
Map generated with 87 territories
MapServer listening on http://localhost:5000
```

### 3. Запустите клиент (Терминал 2)
```bash
cd MapClient
dotnet run
```

Вы должны увидеть:
```
=== MapClient ===
Connecting to server...
Connected to MapServer!
```

## Команды клиента

MapClient предоставляет интерактивную консоль для тестирования сервиса карты:

### Создать объект
```
create <type> <x> <y>
```
Создать новый объект в позиции (x, y).
- `type`: 0 = PlayerBase (база игрока), 1 = Mine (шахта), 2 = Temporary (временный)
- `x, y`: Координаты (0-999)

Пример:
```
> create 0 500 500
[SUCCESS] Created object: abc123 at (500,500)
```

### Удалить объект
```
delete <id>
```
Удалить объект по ID.

Пример:
```
> delete abc123
[SUCCESS] Deleted object: abc123
```

### Переместить объект
```
move <id> <x> <y>
```
Обновить позицию объекта.

Пример:
```
> move abc123 600 600
[SUCCESS] Moved object abc123 to (600,600)
```

### Запросить объекты в области
```
state <x1> <y1> <x2> <y2>
```
Получить все объекты в прямоугольной области.

Пример:
```
> state 0 0 100 100
[INFO] Objects in area (0,0) to (100,100): 3 found
  - ID: obj1, Type: PlayerBase, Pos: (50,50), Owner: 1
  - ID: obj2, Type: Mine, Pos: (75,75), Owner: 2
  - ID: obj3, Type: Temporary, Pos: (90,90), Owner: 1
```

### Запросить регионы в области
```
regions <x1> <y1> <x2> <y2>
```
Получить все регионы карты (территории) в прямоугольной области.

Пример:
```
> regions 0 0 100 100
[INFO] Regions in area (0,0) to (100,100): 5 found
  - ID: 1, Name: Territory_1, Owner: 0
  - ID: 2, Name: Territory_2, Owner: 0
```

### Другие команды
- `help` - Показать доступные команды
- `exit` - Отключиться и выйти

## События в реальном времени (Delta)

Клиент автоматически получает уведомления в реальном времени, когда объекты создаются, обновляются или удаляются любым подключенным клиентом:

```
[DELTA] Object Added: obj123 at (500,500)
[DELTA] Object Updated: obj123 moved to (600,600)
[DELTA] Object Deleted: obj123
```

Чтобы протестировать это:
1. Запустите MapServer в одном терминале
2. Запустите MapClient в двух отдельных терминалах
3. Создайте/переместите/удалите объекты в одном клиенте
4. Наблюдайте, как события появляются в другом клиенте в реальном времени

## Тестирование с несколькими клиентами

Запустите несколько экземпляров MapClient для тестирования broadcast-рассылки событий в реальном времени:

```bash
# Терминал 1: Сервер
cd MapServer && dotnet run

# Терминал 2: Клиент A
cd MapClient && dotnet run

# Терминал 3: Клиент B
cd MapClient && dotnet run

# В Клиенте A создайте объект:
> create 0 500 500

# В Клиенте B вы увидите:
[DELTA] Object Added: <id> at (500,500)
```

## Запуск тестов

```bash
# Запустить все тесты
dotnet test

# Запустить с детальным выводом
dotnet test --verbosity normal
```

Все 96 тестов должны пройти успешно.

## Обзор архитектуры

- **MapServer**: MagicOnion gRPC сервер, размещающий MapService и MapHub
  - Порт: 5000 (HTTP/2)
  - Использует Redis для геопространственного хранения объектов
  - Рассылает события всем подключенным клиентам

- **MapClient**: Интерактивный консольный клиент
  - Подключается через MagicOnion.Client
  - Реализует StreamingHub для событий в реальном времени
  - Использует сериализацию MemoryPack (MagicOnion.Serialization.MemoryPack)

- **MapService**: Сервисный слой, предоставляющий:
  - `IMapService`: Unary RPC для CRUD операций
  - `IMapHub`: StreamingHub для двунаправленной коммуникации
  - Broadcast событий при изменении объектов

- **Redis**: Геопространственное хранилище
  - Хранит объекты с GEO индексацией
  - Используется для запросов по радиусу

## Переменные окружения

### MapServer
- `REDIS_CONNECTION` - Строка подключения к Redis (по умолчанию: `localhost:6379`)
- `SERVER_URL` - URL сервера (по умолчанию: `http://localhost:5000`)
- `ASPNETCORE_ENVIRONMENT` - Окружение ASP.NET Core

### MapClient
- `SERVER_ADDRESS` - Адрес сервера (по умолчанию: `http://localhost:5000`)

## Детали Docker

`docker-compose.yml` включает:
- **redis**: Redis 7 Alpine с проверками здоровья
- **mapserver**: .NET 8 сервер с автоматическим подключением к Redis
- **mapclient**: Опциональный интерактивный клиент (используйте `--profile with-client`)

Все сервисы подключены через bridge-сеть `map-network`.

## Устранение неполадок

### Ошибка "Connection refused"
Redis не запущен. Запустите Redis:
```bash
# macOS
brew services start redis

# Linux
sudo systemctl start redis-server
```

### "Port 5000 already in use"
Другой процесс использует порт 5000. Либо остановите этот процесс, либо измените `MapServer/appsettings.json` для использования другого порта.

### Ошибки сборки
Восстановите NuGet пакеты:
```bash
dotnet restore
dotnet build
```

## Технологии

- .NET 8.0
- MagicOnion 7.0.6 (gRPC фреймворк)
- MemoryPack 1.21.4 (высокопроизводительная сериализация)
- MagicOnion.Serialization.MemoryPack 7.0.6-preview
- Redis 7 (геопространственное хранилище)
- StackExchange.Redis 2.9.32 (клиент Redis)
- xUnit 2.5.3 (фреймворк для тестирования)

## Структура проекта

```
.
├── MapServer/          # Основной сервер
├── MapClient/          # Консольный клиент
├── MapLib/             # Библиотека карты (1000x1000 тайлов, 87 территорий)
├── ObjectsLib/         # Библиотека объектов (Redis GeoIndex)
├── MapService/         # Контракты сервиса (MemoryPack DTOs)
├── UnitTests/          # Юнит-тесты (96 тестов)
├── docker-compose.yml  # Docker Compose конфигурация
└── README.md          # Эта документация
```

## Возможности

### Карта
- Размер: 1000×1000 тайлов
- 87 территорий с произвольными границами
- Типы тайлов: Plain (равнина), Mountain (горы)
- Генерация через алгоритм Вороного
- O(1) доступ к тайлам

### Объекты
- Типы: PlayerBase, Mine, Temporary
- Хранение в Redis с геопространственной индексацией
- Запросы по прямоугольным областям
- Транзакционные операции создания/обновления/удаления

### Сеть
- gRPC через MagicOnion 7.0
- Сериализация MemoryPack (zero-allocation, 20-50% быстрее MessagePack)
- StreamingHub для событий в реальном времени
- Broadcast всем подключенным клиентам