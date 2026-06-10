# Данные о проекте для написания отчета ВКР

Тема ВКР: **Разработка веб-приложения для онлайн-продажи электроники с модульной архитектурой и изолированной бизнес-логикой по принципам DDD**.

Файл содержит собранные сведения о проекте, которые можно использовать как основу для написания основной части отчета: технического задания, аналитического обзора, проектирования и реализации. Финансовый менеджмент и социальная ответственность сюда не включены.

## 1. Назначение проекта

Проект представляет собой веб-приложение интернет-магазина электроники. Система предназначена для онлайн-продажи товаров, ведения каталога электроники, управления корзиной покупателя, оформления заказов, учета складских остатков, администрирования товарных карточек и проверки совместимости товаров.

Продукт можно позиционировать как **StoreFit**: веб-приложение для малого или среднего магазина электроники, которому нужен собственный канал онлайн-продаж и цифровой консультант для помощи покупателю при выборе совместимых товаров.

Основная особенность проекта - не только наличие типовых функций интернет-магазина, но и реализация бизнес-логики в изолированных доменных модулях по принципам Domain-Driven Design. Это позволяет рассматривать проект не как простую витрину товаров, а как модульную предметную систему с отдельными ограниченными контекстами.

## 2. Целевая аудитория и предметная область

Целевая аудитория системы:

- покупатели электроники, которые хотят выбрать товар, добавить его в корзину и оформить заказ;
- менеджеры магазина, которые управляют товарами, изображениями, остатками и правилами совместимости;
- малые и средние магазины электроники, которым нужен собственный канал продаж и инструмент консультации покупателей.

Предметная область связана с онлайн-продажей электроники. Для нее характерны следующие особенности:

- товары имеют технические характеристики, влияющие на совместимость;
- покупателю часто требуется помощь при выборе комплектующих и аксессуаров;
- важно учитывать наличие товара на складе;
- заказ должен быть связан с резервированием остатков;
- изменение цены, доступности или изображения товара должно отражаться в связанных модулях;
- менеджеру нужны инструменты импорта, редактирования и пополнения остатков.

Типовые вопросы покупателей, которые учитывает предметная область:

- подойдет ли выбранный SSD к ноутбуку;
- совместима ли оперативная память с материнской платой;
- подходит ли процессор к выбранной плате;
- хватит ли блока питания для выбранной видеокарты;
- какой аксессуар подходит к конкретной модели устройства.

## 3. Общая характеристика решения

Система реализована как модульный монолит. Внешне backend работает как единое ASP.NET Core API, но внутри разделен на независимые бизнес-модули. Каждый модуль содержит собственные слои Domain, Application, Infrastructure и API либо их релевантную часть.

Основная идея архитектуры:

- бизнес-логика находится внутри доменных моделей, а не в контроллерах;
- модули не обращаются напрямую к внутренним слоям других модулей;
- взаимодействие между модулями выполняется через контрактные проекты и доменные события;
- каждый модуль использует собственный DbContext и отдельную схему PostgreSQL;
- межмодульные события публикуются через Outbox и обрабатываются с учетом идемпотентности через Inbox.

## 4. Технологический стек

Backend:

- ASP.NET Core;
- C#;
- .NET 9;
- Entity Framework Core;
- PostgreSQL;
- Npgsql.EntityFrameworkCore.PostgreSQL;
- MediatR;
- CSharpFunctionalExtensions;
- JWT-аутентификация;
- BCrypt.Net-Next для хеширования паролей;
- MailKit для email-уведомлений;
- ClosedXML для импорта и экспорта товаров;
- Swagger / Swashbuckle для документации API.

Frontend:

- React 19;
- TypeScript;
- Vite;
- React Router;
- TanStack React Query;
- styled-components;
- lucide-react.

Инфраструктура:

- Docker;
- Docker Compose;
- Nginx для frontend-контейнера;
- PostgreSQL в отдельном контейнере;
- volume для хранения данных БД;
- volume/директория для изображений товаров.

Тестирование:

- xUnit;
- Microsoft.NET.Test.Sdk;
- coverlet.collector;
- архитектурные тесты изоляции слоев;
- доменные и application-тесты ключевых сценариев.

## 5. Структура backend-решения

Файл решения: `backend/Store.sln`.

Основные проекты backend:

- `Store.App` - точка входа приложения, настройка ASP.NET Core, регистрация модулей, middleware, миграции, seed-данные;
- `Store.SharedKernel` - общие строительные блоки, включая AggregateRoot и базовые доменные события;
- `Store.EventOutbox` - общий механизм Outbox/Inbox для доменных событий;
- `Store.Identity` - пользователи, регистрация, вход, роли, JWT;
- `Store.Identity.Contracts` - события и контракты модуля пользователей;
- `Store.Catalog` - каталог товаров, категории, характеристики, изображения, импорт/экспорт;
- `Store.Catalog.Contracts` - события каталога;
- `Store.Carts` - корзина покупателя, checkout, product cache;
- `Store.Carts.Contracts` - событие оформления корзины;
- `Store.Ordering` - заказы, статусы, оплата, реакция на checkout и резервирование;
- `Store.Ordering.Contracts` - события заказов;
- `Store.Inventory` - складские остатки, резервирование, списание, освобождение резерва;
- `Store.Inventory.Contracts` - события склада и интерфейс записи остатков;
- `Store.Notifications` - email-уведомления и собственный outbox отправки писем;
- `Store.Consulting` - цифровой консультант, правила совместимости, рекомендации;
- `Store.Consulting.Contracts` - события консультационного модуля;
- `Store.Tests` - тесты доменной логики, application-сценариев, инфраструктуры и архитектуры.

## 6. Структура frontend

Frontend расположен в `frontend`.

Основные каталоги:

- `src/api` - API-клиент, типы ответов и запросов;
- `src/app` - маршруты приложения, провайдеры, основной controller hook;
- `src/components` - общие компоненты интерфейса;
- `src/hooks` - хуки для данных каталога, корзины, заказов, админки и консультаций;
- `src/pages` - страницы приложения;
- `src/styles` - глобальные стили;
- `src/types` - общие типы навигации;
- `src/utils` - форматирование, статусы заказов, спецификации.

Основные страницы frontend:

- `/` - каталог товаров;
- `/auth` - регистрация и вход;
- `/products/:productId` - карточка товара;
- `/cart` - корзина;
- `/orders` - заказы пользователя;
- `/admin` - административная панель для роли Manager.

API-клиент frontend хранит JWT и роль пользователя в `localStorage`, добавляет Bearer-токен к защищенным запросам, обрабатывает ошибки 401 и 403, поддерживает JSON-запросы и FormData для загрузки файлов.

## 7. Ограниченные контексты и модули

### 7.1. Identity

Назначение: управление пользователями, регистрацией, входом и ролями.

Ключевые элементы:

- агрегат `User`;
- value object `Email`;
- enum `UserRole`;
- сервисы `PasswordHasher`, `JwtProvider`;
- событие `UserRegisteredEvent`;
- API: `/Users/register`, `/Users/login`, `/Users/logout`, `/Users`.

Роли:

- `Customer` - покупатель;
- `Manager` - менеджер, которому доступна административная часть.

### 7.2. Catalog

Назначение: управление каталогом электроники, категориями, товарами, характеристиками, изображениями и доступностью.

Ключевые элементы:

- агрегат `Product`;
- агрегат `Category`;
- сущность `ProductImage`;
- value objects `Sku`, `Money`, `ProductSpecification`;
- enum `ElectronicsCategoryCode`;
- доменный сервис `ElectronicsCategorySpecificationRules`;
- read-модель доступности `ProductAvailability`;
- импорт и экспорт товаров через Excel.

Доменные события:

- `ProductCreatedEvent`;
- `ProductPriceChangedEvent`;
- `ProductDeletedEvent`;
- `ProductMainImageChangedEvent`.

API:

- `/Products`;
- `/Products/export`;
- `/Products/import/preview`;
- `/Products/import`;
- `/Products/{id}`;
- `/Products/{id}/images`;
- `/Categories`.

Особенность для отчета: каталог содержит не просто поля товара, а предметные правила для электроники. Для разных категорий требуются разные характеристики, например socket, chipset, memoryType, formFactor, interface, capacity, connectorType.

### 7.3. Carts

Назначение: управление корзиной покупателя и запуск процесса оформления заказа.

Ключевые элементы:

- агрегат `Cart`;
- сущность `CartItem`;
- value objects `CheckoutDetails`, `RecipientName`, `PhoneNumber`, `DeliveryAddress`, `DeliveryMethod`, `PaymentMethod`, `ProductInfo`;
- repository `CartRepository`;
- `ProductCacheRepository`;
- `CustomerCartMutationLocks` для блокировки одновременных изменений корзины одного пользователя;
- `CheckoutOrderTracker` для связи checkout-события с созданным заказом.

Доменные события:

- `CartCheckedOutEvent`.

API:

- `GET /Cart`;
- `POST /Cart/items`;
- `PUT /Cart/items/{cartItemId}/quantity`;
- `DELETE /Cart/items/{cartItemId}`;
- `POST /Cart/checkout`.

Особенность для отчета: корзина не создает заказ напрямую. При checkout она переходит в состояние `IsCheckoutPending`, сохраняет `PendingCheckoutId`, публикует событие `CartCheckedOutEvent` и ожидает результата обработки другими модулями. Это снижает связанность корзины и заказов.

### 7.4. Ordering

Назначение: создание и сопровождение заказов.

Ключевые элементы:

- агрегат `Order`;
- value object `OrderedProduct`;
- value objects для контактных и платежных данных;
- enum `OrderStatus`;
- `MockPaymentGateway`;
- обработчики событий корзины и склада.

Статусы заказа:

- `AwaitingStock` - заказ создан и ожидает резервирования товара;
- `Unpaid` - склад подтвердил резерв, заказ можно оплачивать;
- `Paid` - заказ оплачен;
- `Cancelled` - заказ отменен;
- `Rejected` - заказ отклонен из-за невозможности резервирования.

Доменные события:

- `OrderStockReservationRequestedEvent`;
- `OrderCreatedEvent`;
- `OrderRejectedEvent`;
- `OrderPaidEvent`;
- `OrderCancelledEvent`.

API:

- `GET /Orders/{orderId}`;
- `GET /Orders/my`;
- `POST /Orders/{orderId}/payment`;
- `POST /Orders/{orderId}/cancel`.

Особенность для отчета: агрегат заказа инкапсулирует допустимые переходы статусов. Например, нельзя оплатить заказ, который ожидает резервирования, отменен или отклонен.

### 7.5. Inventory

Назначение: учет количества товаров, резервирование, списание и освобождение резерва.

Ключевые элементы:

- агрегат `StockItem`;
- сущность `StockReservation`;
- `InventoryStockWriter`;
- `StockItemRepository`;
- concurrency token в миграциях склада.

Доменные события:

- `StockChangedEvent`;
- `StockDepletedEvent`;
- `OrderStockReservedEvent`;
- `OrderStockReservationRejectedEvent`.

API:

- `GET /Inventory/{productId}`;
- `POST /Inventory/replenish`.

Особенность для отчета: склад работает с двумя величинами: `Quantity` и `Reserved`. Доступное количество вычисляется как `Available = Quantity - Reserved`. При оплате заказа резерв списывается, при отмене освобождается.

### 7.6. Notifications

Назначение: формирование и отправка email-уведомлений.

Ключевые элементы:

- `OutboxMessageEntity`;
- `NotificationOutbox`;
- `EmailNotificationSender`;
- `OutboxProcessorWorker`;
- `SmtpOptions`;
- обработчики событий регистрации и заказов.

Обрабатываемые события:

- `UserRegisteredEvent`;
- `OrderCreatedEvent`;
- `OrderPaidEvent`;
- `OrderCancelledEvent`;
- `OrderRejectedEvent`.

Особенность для отчета: уведомления используют собственную очередь Outbox с количеством попыток, backoff и dead-letter, чтобы отправка писем не ломала бизнес-транзакции.

### 7.7. Consulting

Назначение: цифровой консультант, проверка совместимости товаров и выдача рекомендаций.

Ключевые элементы:

- доменный сервис `CompatibilityChecker`;
- сущности `CompatibilityRule`, `CompatibilityCondition`, `CompatibilityIssue`, `ConsultationResult`, `ProductRecommendation`;
- value object `ConsultationProduct`;
- read repository для чтения товаров каталога;
- репозиторий правил совместимости;
- сохранение результатов консультаций.

API:

- `POST /Consulting/cart/check`;
- `GET /Consulting/products/{productId}/recommendations`;
- `GET /Consulting/rules`;
- `POST /Consulting/rules`;
- `PUT /Consulting/rules/{ruleId}`;
- `DELETE /Consulting/rules/{ruleId}`;
- `POST /Consulting/rules/test`.

Операторы правил:

- `Equals`;
- `NotEquals`;
- `In`;
- `GreaterThanOrEqual`;
- `LessThanOrEqual`.

Уровни результата:

- `Ok`;
- `Warning`;
- `Error`;
- `Unknown`.

Типы рекомендаций:

- `Alternative`;
- `Accessory`;
- `RequiredPart`.

Особенность для отчета: консультационный модуль является отличительной функцией проекта. Он анализирует характеристики товаров и позволяет описывать совместимость через настраиваемые правила, а не через жестко зашитые проверки.

## 8. Реализация DDD-подхода

В проекте используются следующие элементы DDD:

- ограниченные контексты: Identity, Catalog, Carts, Ordering, Inventory, Notifications, Consulting;
- агрегаты: User, Product, Category, Cart, Order, StockItem;
- сущности: CartItem, ProductImage, StockReservation, CompatibilityRule, CompatibilityIssue и др.;
- объекты-значения: Email, Sku, Money, ProductSpecification, OrderedProduct, CheckoutDetails, PhoneNumber, DeliveryAddress, PaymentMethod;
- доменные сервисы: CompatibilityChecker, ElectronicsCategorySpecificationRules;
- доменные события: события каталога, корзины, заказов, склада, пользователей;
- репозитории для доступа к агрегатам;
- application layer с командами, запросами и обработчиками CQRS;
- инфраструктурный слой с EF Core, DbContext, миграциями и реализациями репозиториев;
- API-слой с контроллерами.

Важно для отчета: бизнес-правила находятся в доменных моделях. Например:

- `Product.Create` и `Product.Update` валидируют SKU, цену, бренд, модель, гарантию и характеристики;
- `Cart.Checkout` проверяет пустую корзину, данные получателя и переводит корзину в pending-состояние;
- `Order.MarkAsPaid`, `Order.Cancel`, `Order.ConfirmStockReserved`, `Order.RejectStockReservation` контролируют допустимые переходы статусов;
- `StockItem.Reserve`, `StockItem.Release`, `StockItem.Deduct` контролируют складской резерв;
- `CompatibilityChecker.Check` применяет активные правила совместимости к товарам.

## 9. Изоляция бизнес-логики и модулей

Изоляция реализована на нескольких уровнях:

1. Каждый бизнес-модуль выделен в отдельный проект.
2. Контрактные события вынесены в отдельные проекты `*.Contracts`.
3. Модули могут ссылаться на контракты других модулей, но не на их Domain, Application, Infrastructure или API.
4. Domain layer не зависит от EF Core, ASP.NET Core, Npgsql, Infrastructure, Application и API.
5. Application layer не зависит от Infrastructure и веб-деталей.
6. API layer не зависит от доменной реализации и инфраструктуры напрямую.
7. SharedKernel не зависит от feature-модулей и инфраструктурных пакетов.

Эти правила проверяются тестами в `backend/Store.Tests/Architecture/LayerIsolationTests.cs`.

Для отчета можно подчеркнуть, что архитектурная изоляция не только декларируется, но и автоматически проверяется тестами.

## 10. Межмодульное взаимодействие через события

Общий механизм событий находится в проекте `Store.EventOutbox`.

Ключевые классы:

- `DomainEventOutboxMessage` - сохраненное сообщение доменного события;
- `ProcessedDomainEvent` - отметка об обработанном событии;
- `EfDomainEventOutbox` - запись событий в outbox;
- `EfDomainEventInbox` - проверка и запись обработанных событий;
- `DomainEventOutboxProcessor<TDbContext>` - фоновая публикация событий;
- `DomainEventSerializer` - сериализация и десериализация событий.

Основные свойства механизма:

- события сохраняются в БД в рамках транзакции модуля;
- фоновый процессор выбирает необработанные события пачками;
- события публикуются через MediatR;
- при ошибке публикации выполняются повторные попытки;
- после превышения числа попыток сообщение может быть переведено в dead-letter;
- Inbox предотвращает повторную обработку одного и того же события одним потребителем.

Outbox/Inbox используется в модулях:

- Catalog;
- Carts;
- Ordering;
- Inventory;
- Identity;
- Notifications.

## 11. Ключевой сценарий: добавление товара в корзину

Сценарий:

1. Пользователь нажимает кнопку добавления товара в корзину.
2. Frontend отправляет `POST /Cart/items`.
3. API получает `CustomerId` из JWT.
4. `AddItemCommandHandler` обращается к `ProductCacheRepository`.
5. Если товар не найден или недоступен, возвращается ошибка.
6. Для пользователя берется блокировка `CustomerCartMutationLocks`.
7. Загружается существующая корзина или создается новая.
8. Агрегат `Cart` добавляет новый товар или увеличивает количество существующего.
9. Корзина сохраняется через `CartRepository`.
10. Клиент получает обновленное состояние корзины.

Почему сценарий важен:

- используется product cache, чтобы корзина не зависела напрямую от внутренней модели каталога;
- есть защита от одновременных изменений корзины одного пользователя;
- бизнес-правила добавления находятся в агрегате `Cart`, а не в контроллере.

## 12. Ключевой сценарий: оформление заказа

Сценарий является центральным для реализации и хорошо подходит для подробного описания в главе реализации.

Этапы:

1. Пользователь оформляет корзину через `POST /Cart/checkout`.
2. `CheckoutCommandHandler` загружает корзину покупателя.
3. Агрегат `Cart` проверяет:
   - корзина не находится в pending-состоянии;
   - корзина не пуста;
   - email покупателя заполнен;
   - контактные данные, адрес, способ доставки и оплаты корректны.
4. Корзина формирует снимок товаров и событие `CartCheckedOutEvent`.
5. Корзина переводится в состояние `IsCheckoutPending`.
6. Событие сохраняется в outbox модуля корзины.
7. `CartCheckedOutEventHandler` в модуле Ordering создает агрегат `Order`.
8. Заказ создается в статусе `AwaitingStock`.
9. Заказ публикует событие `OrderStockReservationRequestedEvent`.
10. `OrderStockReservationRequestedEventHandler` в модуле Inventory проверяет остатки.
11. Если товара достаточно, склад резервирует позиции и публикует `OrderStockReservedEvent`.
12. Если товара недостаточно, склад публикует `OrderStockReservationRejectedEvent`.
13. При успешном резервировании заказ переходит в статус `Unpaid` и публикует `OrderCreatedEvent`.
14. Корзина получает `OrderCreatedEvent`, очищается и снимает pending-состояние.
15. При отклонении резервирования заказ переходит в `Rejected`, а корзина освобождает pending-состояние.

Почему сценарий важен:

- оформление заказа разделено между тремя модулями: Carts, Ordering, Inventory;
- связь между модулями построена через события, а не через прямые вызовы внутренних сервисов;
- корзина очищается только после успешного создания заказа;
- недостаток товара не приводит к потере корзины;
- обработчики событий идемпотентны.

## 13. Ключевой сценарий: оплата и списание остатков

Этапы:

1. Пользователь вызывает `POST /Orders/{orderId}/payment`.
2. `MarkOrderPaidCommandHandler` загружает заказ.
3. Проверяется принадлежность заказа пользователю.
4. Проверяется статус заказа.
5. Если заказ ожидает резервирования, отклонен или отменен, оплата запрещается.
6. `MockPaymentGateway` имитирует платеж.
7. Агрегат `Order` переводится в статус `Paid`.
8. Публикуется `OrderPaidEvent`.
9. Модуль Inventory обрабатывает событие и списывает ранее зарезервированные товары через `StockItem.Deduct`.
10. Склад публикует `StockChangedEvent`, а Catalog обновляет read-модель доступности.

Почему сценарий важен:

- списание склада происходит после оплаты, а не на этапе создания заказа;
- резерв защищает товар от продажи другому пользователю;
- каталог получает обновленную доступность через событие склада.

## 14. Ключевой сценарий: проверка совместимости товаров

Этапы:

1. Frontend отправляет список товаров в `POST /Consulting/cart/check`.
2. Модуль Consulting читает данные товаров и их характеристики.
3. Загружаются активные правила совместимости.
4. `CompatibilityChecker` перебирает правила и пары товаров соответствующих категорий.
5. Для каждой пары сравниваются характеристики по оператору правила.
6. При проблеме создается `CompatibilityIssue`.
7. При необходимости создается рекомендация.
8. Возвращается результат консультации со статусом `Ok`, `Warning`, `Error` или `Unknown`.

Почему сценарий важен:

- система помогает покупателю выбрать совместимые товары;
- правила совместимости настраиваются менеджером;
- логика проверки отделена от UI и инфраструктуры;
- это отличительная функция проекта по сравнению с типовым интернет-магазином.

## 15. База данных и схемы PostgreSQL

PostgreSQL используется как основное хранилище.

Каждый модуль использует собственную схему:

- `identity`;
- `catalog`;
- `cart`;
- `ordering`;
- `inventory`;
- `notifications`;
- `consulting`.

Каждый модуль имеет свой DbContext:

- `IdentityDbContext`;
- `CatalogDbContext`;
- `CartDbContext`;
- `OrderingDbContext`;
- `InventoryDbContext`;
- `NotificationsDbContext`;
- `ConsultingDbContext`.

Логическая структура данных:

- `Users` - пользователи;
- `Categories` - категории каталога;
- `Products` - товары;
- `ProductSpecifications` - характеристики товаров;
- `ProductImages` - изображения товаров;
- `ProductAvailability` - доступность товара для каталога;
- `ProductCache` - кэш товаров в корзине;
- `Carts` - корзины;
- `CartItems` - позиции корзины;
- `Orders` - заказы;
- `OrderedProducts` - товары в заказе;
- `StockItems` - складские позиции;
- `StockReservations` - резервы по заказам;
- `OutboxMessages` - email-сообщения;
- служебные таблицы outbox/inbox доменных событий.

Важный архитектурный момент: между схемами разных модулей прямые внешние ключи не используются как основной способ связи. Модули связываются логически по идентификаторам и через события.

## 16. API системы

Основные группы endpoint:

Identity:

- `POST /Users/register`;
- `POST /Users/login`;
- `POST /Users/logout`;
- `GET /Users`.

Catalog:

- `GET /Products`;
- `POST /Products`;
- `GET /Products/{id}`;
- `PUT /Products/{id}`;
- `DELETE /Products/{id}`;
- `GET /Products/category/{categoryId}`;
- `GET /Products/export`;
- `POST /Products/import/preview`;
- `POST /Products/import`;
- `GET /Products/{id}/images`;
- `POST /Products/{id}/images`;
- `PUT /Products/{id}/images/{imageId}/main`;
- `DELETE /Products/{id}/images/{imageId}`;
- `GET /Categories`;
- `POST /Categories`;
- `PUT /Categories/{id}`;
- `DELETE /Categories/{id}`.

Carts:

- `GET /Cart`;
- `POST /Cart/items`;
- `PUT /Cart/items/{cartItemId}/quantity`;
- `DELETE /Cart/items/{cartItemId}`;
- `POST /Cart/checkout`.

Ordering:

- `GET /Orders/{orderId}`;
- `GET /Orders/my`;
- `POST /Orders/{orderId}/payment`;
- `POST /Orders/{orderId}/cancel`.

Inventory:

- `GET /Inventory/{productId}`;
- `POST /Inventory/replenish`.

Consulting:

- `POST /Consulting/cart/check`;
- `GET /Consulting/products/{productId}/recommendations`;
- `GET /Consulting/rules`;
- `POST /Consulting/rules`;
- `PUT /Consulting/rules/{ruleId}`;
- `DELETE /Consulting/rules/{ruleId}`;
- `POST /Consulting/rules/test`.

## 17. Контейнеризация и запуск

Файл инфраструктуры: `compose.yaml`.

Сервисы Docker Compose:

- `store.app` - ASP.NET Core API, порт `8080`;
- `store.frontend` - Nginx + React frontend, порт `3000`;
- `Database` - PostgreSQL, внутренний порт `5432`, проброс на `127.0.0.1:5432`;
- `database_volume` - постоянное хранение данных PostgreSQL;
- `store_network` - внутренняя сеть контейнеров.

При запуске backend:

- регистрирует модули;
- подключает CORS;
- подключает JWT-аутентификацию;
- подключает rate limiting для login;
- применяет миграции для всех DbContext;
- наполняет демонстрационные данные электроники;
- синхронизирует product cache корзины.

## 18. Безопасность

Реализованные механизмы:

- регистрация и вход пользователей;
- JWT-токен;
- хранение сессии на frontend;
- роли `Customer` и `Manager`;
- защита административной страницы на frontend по роли;
- Bearer-токен в API-запросах;
- cookie policy с HttpOnly и SameSite;
- rate limiting для login-запросов;
- Swagger с Bearer-аутентификацией;
- CORS-настройки.

Для отчета можно указать, что основное разграничение доступа связано с ролью менеджера, который может управлять каталогом, импортом, остатками и правилами совместимости.

## 19. Тестирование

В проекте есть тестовый проект `Store.Tests`.

Группы тестов:

- `Domain` - тесты доменных моделей;
- `Application` - тесты команд, запросов и сценариев;
- `API` - тесты API консультационного модуля;
- `Architecture` - тесты изоляции слоев и модулей;
- `Infrastructure` - тесты миграций и хранения изображений.

Примеры тестовых файлов:

- `CartTests.cs`;
- `OrderTests.cs`;
- `InventoryTests.cs`;
- `ProductTests.cs`;
- `UserTests.cs`;
- `CompatibilityCheckerTests.cs`;
- `ElectronicsCategorySpecificationRulesTests.cs`;
- `OnlinePurchaseFlowTests.cs`;
- `AddItemCommandHandlerTests.cs`;
- `CheckCartCompatibilityQueryHandlerTests.cs`;
- `MarkOrderPaidCommandHandlerTests.cs`;
- `ReleaseStaleCheckoutsCommandHandlerTests.cs`;
- `EventIdempotencyTests.cs`;
- `CartEventHandlerIdempotencyTests.cs`;
- `LayerIsolationTests.cs`;
- `MigrationSmokeTests.cs`.

Что важно подчеркнуть в отчете:

- проверяется не только UI или API, но и доменная логика;
- есть тесты полного сценария онлайн-покупки;
- есть тесты идемпотентности событий;
- есть тесты архитектурной изоляции, подтверждающие соответствие модульной архитектуре.

## 20. Существующие схемы и графические материалы

В проекте уже есть файлы, которые можно использовать как основу для рисунков в отчете:

- `docs/architecture.puml` - архитектура онлайн-магазина;
- `docs/technical-solution.puml` - техническое решение с Docker Compose;
- `docs/database-logical-schema.puml` - логическая схема базы данных;
- `docs/add-to-cart-sequence.puml` - сценарий добавления товара в корзину;
- `docs/order-flow-sequence.puml` - сценарий создания заказа;
- `technical-architecture-storefit.svg` - схема технической архитектуры;
- `competitor-analysis-storefit.svg` - схема анализа конкурентов;
- `custdev-infographic.svg` - инфографика customer development;
- `org-structure-storefit.svg` - организационная структура;
- `product-differentiation-modules.md` - материалы по позиционированию StoreFit.

Рекомендуемые рисунки для основной части отчета:

- общая архитектура веб-приложения;
- схема модульного монолита;
- диаграмма ограниченных контекстов;
- логическая схема базы данных;
- диаграмма последовательности добавления товара в корзину;
- диаграмма последовательности оформления заказа;
- схема Outbox/Inbox;
- схема статусов заказа;
- схема работы цифрового консультанта;
- скриншоты каталога, корзины, оформления заказа, заказов и админ-панели.

## 21. Что лучше раскрывать в главе реализации

Так как в реализации не нужно описывать всю систему подряд, наиболее ценные и сложные части:

1. Реализация модульной архитектуры backend.
2. Изоляция Domain, Application, Infrastructure и API.
3. Реализация агрегатов `Cart`, `Order`, `StockItem`, `Product`.
4. Реализация сценария checkout через доменные события.
5. Реализация Outbox/Inbox и идемпотентной обработки.
6. Реализация резервирования и списания складских остатков.
7. Реализация product cache в корзине.
8. Реализация правил характеристик электроники.
9. Реализация цифрового консультанта и проверки совместимости.
10. Реализация архитектурных и доменных тестов.

Менее важные части, которые можно упомянуть кратко:

- обычная верстка страниц;
- типовые CRUD-операции категорий;
- базовая настройка React;
- стандартные DTO и маппинги;
- простые контроллеры, если они не содержат архитектурно значимой логики.

## 22. Возможная логика основной части отчета

Рекомендуемая связь глав:

1. В техническом задании описать необходимость интернет-магазина электроники с каталогом, корзиной, заказами, складом и консультационным механизмом.
2. В аналитическом обзоре показать, почему электроника сложнее обычных товаров: характеристики, совместимость, ошибки выбора, складские остатки.
3. Затем обосновать модульную архитектуру и DDD: предметная область имеет разные зоны ответственности, поэтому нужна изоляция логики.
4. В проектировании выделить ограниченные контексты: Catalog, Carts, Ordering, Inventory, Identity, Notifications, Consulting.
5. Затем спроектировать доменную модель и межмодульные события.
6. В реализации раскрыть самые сложные механизмы: checkout, резервирование, Outbox/Inbox, цифровой консультант и тесты.

## 23. Файлы, на которые можно ссылаться при написании

Backend:

- `backend/Store.App/Program.cs`;
- `backend/Store.Catalog/CatalogModule.cs`;
- `backend/Store.Carts/CartModule.cs`;
- `backend/Store.Ordering/OrderingModule.cs`;
- `backend/Store.Inventory/InventoryModule.cs`;
- `backend/Store.Identity/IdentityModule.cs`;
- `backend/Store.Notifications/NotificationsModule.cs`;
- `backend/Store.Consulting/ConsultingModule.cs`;
- `backend/Store.Catalog/Domain/Entities/Product.cs`;
- `backend/Store.Carts/Domain/Aggregates/Cart.cs`;
- `backend/Store.Ordering/Domain/Aggregates/Order.cs`;
- `backend/Store.Inventory/Domain/Aggregates/StockItem.cs`;
- `backend/Store.Consulting/Domain/Services/CompatibilityChecker.cs`;
- `backend/Store.Catalog/Domain/Services/ElectronicsCategorySpecificationRules.cs`;
- `backend/Store.EventOutbox/Infrastructure/Services/DomainEventOutboxProcessor.cs`;
- `backend/Store.EventOutbox/Infrastructure/Serialization/DomainEventSerializer.cs`;
- `backend/Store.Tests/Architecture/LayerIsolationTests.cs`.

Frontend:

- `frontend/src/app/AppRoutes.tsx`;
- `frontend/src/api/client.ts`;
- `frontend/src/api/types.ts`;
- `frontend/src/pages/shop/ShopView.tsx`;
- `frontend/src/pages/product/ProductDetailsView.tsx`;
- `frontend/src/pages/cart/CartView.tsx`;
- `frontend/src/pages/orders/OrdersView.tsx`;
- `frontend/src/pages/admin/AdminView.tsx`;
- `frontend/src/pages/admin/ConsultingRulesPanel.tsx`;
- `frontend/src/pages/admin/ProductImportExportPanel.tsx`.

Инфраструктура и схемы:

- `compose.yaml`;
- `docs/architecture.puml`;
- `docs/technical-solution.puml`;
- `docs/database-logical-schema.puml`;
- `docs/add-to-cart-sequence.puml`;
- `docs/order-flow-sequence.puml`.

