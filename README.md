# Financial Planner API

API для управления финансовым планированием с поддержкой планирования доходов и расходов, отслеживания факта и статистики по категориям.

## Технологии

- .NET 8.0
- ASP.NET Core Web API
- Entity Framework Core
- MySQL (Pomelo.EntityFrameworkCore.MySql)
- JWT Authentication

## Настройка базы данных

### 1. Подключение к MySQL серверу

```bash
mysql -h 89.104.67.36 -P 3306 -u root -p
```

### 2. Выполнение скриптов создания базы данных

Выполните скрипты в следующем порядке:

```bash
# Создание базы данных и пользователя
mysql -h 89.104.67.36 -P 3306 -u root -p < docs/db/01_create_database_and_user.sql

# Создание таблиц
mysql -h 89.104.67.36 -P 3306 -u root -p < docs/db/02_create_tables.sql
```

Или выполните полный скрипт:

```bash
mysql -h 89.104.67.36 -P 3306 -u root -p < docs/db/setup_complete.sql
```

**Важно:** Не забудьте заменить `your_secure_password` на реальный надежный пароль в файле `01_create_database_and_user.sql`.

### 3. Настройка строки подключения

Обновите файл `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "Database": "Server=89.104.67.36;Port=3306;Database=financial_planner;Uid=financial_planner_user;Pwd=your_secure_password;"
  }
}
```

### 4. Настройка JWT

Обновите секцию JWT в `appsettings.json`:

```json
{
  "Jwt": {
    "Key": "your-super-secret-key-minimum-32-characters-long-for-security",
    "Issuer": "FinancialPlanner",
    "Audience": "FinancialPlanner",
    "ExpirationDays": 7
  }
}
```

**Важно:** Используйте надежный секретный ключ длиной минимум 32 символа для продакшена.

## Запуск приложения

```bash
cd FinancialPlanner.Api
dotnet restore
dotnet run
```

API будет доступен по адресу: `https://localhost:5001` или `http://localhost:5000`

Swagger UI: `https://localhost:5001/swagger`

## API Endpoints

### Аутентификация

- `POST /api/auth/register` - Регистрация нового пользователя
- `POST /api/auth/login` - Вход и получение JWT токена

### Категории (требуется авторизация)

- `GET /api/categories` - Получить все категории
- `GET /api/categories/{id}` - Получить категорию по ID
- `POST /api/categories` - Создать категорию
- `PUT /api/categories/{id}` - Обновить категорию
- `DELETE /api/categories/{id}` - Удалить категорию

### Расходы (требуется авторизация)

- `GET /api/expenses?year={year}&month={month}` - Получить расходы за месяц
- `POST /api/expenses` - Создать расход
- `PUT /api/expenses/{id}` - Обновить расход
- `DELETE /api/expenses/{id}` - Удалить расход

### Доходы (требуется авторизация)

- `GET /api/income?year={year}` - Получить доходы за год
- `POST /api/income` - Создать доход
- `PUT /api/income/{id}` - Обновить доход
- `DELETE /api/income/{id}` - Удалить доход

### Планы месяца (требуется авторизация)

- `GET /api/monthlyplans/{year}/{month}` - Получить план на месяц
- `GET /api/monthlyplans/year/{year}` - Получить планы на год
- `POST /api/monthlyplans` - Создать план
- `PUT /api/monthlyplans/{id}` - Обновить план
- `DELETE /api/monthlyplans/{id}` - Удалить план

### Сводки (требуется авторизация)

- `GET /api/summary?year={year}` - Получить сводки за год

## Использование JWT токена

После успешного входа, используйте полученный токен в заголовке `Authorization`:

```
Authorization: Bearer {your_jwt_token}
```

## Структура проекта

```
api/
├── FinancialPlanner.Api/          # Веб API
├── FinancialPlanner.Application/   # Бизнес-логика и DTOs
├── FinancialPlanner.Domain/        # Доменные модели
├── FinancialPlanner.Infrastructure/# Реализация персистентности
└── docs/
    └── db/                         # SQL скрипты
```

## Миграции базы данных

При необходимости создания миграций Entity Framework:

```bash
cd FinancialPlanner.Infrastructure
dotnet ef migrations add MigrationName --startup-project ../FinancialPlanner.Api
dotnet ef database update --startup-project ../FinancialPlanner.Api
```

## Лицензия

MIT

