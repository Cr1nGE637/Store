# План реализации модуля цифрового консультанта

## 1. Цель модуля

Цифровой консультант должен:

1. Проверять совместимость товаров между собой.
2. Предупреждать покупателя об ошибках в корзине.
3. Объяснять, почему товар подходит или не подходит.
4. Предлагать совместимые альтернативы и аксессуары.
5. Помогать выбрать товар по сценарию использования.
6. Давать менеджеру возможность поддерживать экспертные правила.

## 2. MVP-сценарии

Для первой версии лучше реализовать не универсальный ИИ, а набор понятных и проверяемых сценариев.

1. Проверка корзины на несовместимость.

   Пример: материнская плата с сокетом `AM4` и процессор `LGA1700` несовместимы.

2. Рекомендация аксессуаров.

   Пример: к ноутбуку предложить SSD, RAM, зарядку или сумку.

3. Подбор альтернатив.

   Пример: выбранный товар отсутствует на складе или не подходит, система предлагает похожие совместимые товары.

4. Объяснение совместимости на карточке товара.

   Пример: "Эта RAM подходит, потому что тип памяти DDR4 совпадает с требованиями выбранной материнской платы".

5. Предупреждение перед оформлением заказа.

   Пример: "В корзине есть товары, которые могут быть несовместимы. Проверьте перед оформлением".

## 3. Архитектурное место модуля

Добавить новый backend-модуль:

```text
backend/
  Store.Consulting/
    API/
    Application/
    Domain/
    Infrastructure/
  Store.Consulting.Contracts/
```

Подключить модуль в `backend/Store.App/Program.cs`:

```csharp
builder.Services.AddConsultingModule(builder.Configuration);
```

Модуль должен быть отдельным bounded context, как `Catalog`, `Carts`, `Ordering`, `Inventory`, `Identity` и `Notifications`.

## 4. Доменная модель

Основные доменные сущности:

1. `CompatibilityRule` - правило совместимости.
2. `ConsultationSession` - сессия консультации пользователя.
3. `ConsultationResult` - результат проверки.
4. `Recommendation` - рекомендация товара.
5. `CompatibilityIssue` - найденная проблема.

Типы правил:

```text
ExactMatchRule        - значения должны совпадать, например socket
AllowedValuesRule     - значение должно входить в список
RequiredAccessoryRule - нужен сопутствующий товар
PowerBudgetRule       - расчет мощности БП для сборки
CategoryPairRule      - правило между двумя категориями
```

## 5. База данных

Создать отдельную схему, например `consulting`.

Полный набор таблиц:

```text
consulting.compatibility_rules
consulting.rule_conditions
consulting.rule_actions
consulting.consultation_sessions
consulting.consultation_messages
consulting.consultation_results
consulting.recommendation_events
```

Для MVP достаточно начать с трех таблиц:

```text
compatibility_rules
consultation_sessions
consultation_results
```

В `compatibility_rules` хранить:

```text
id
name
source_category_id
target_category_id
source_specification_key
target_specification_key
operator
severity
message_template
is_active
created_at
updated_at
```

## 6. Межмодульные контракты

Создать проект `Store.Consulting.Contracts`.

Нужные DTO и события:

```text
ProductCompatibilityCheckedEvent
CartConsultationRequestedEvent
CartConsultationCompletedEvent
ConsultationIssueDetectedEvent
RecommendationCreatedEvent
```

Для MVP события можно не публиковать сразу, а сделать REST API. Но контракты лучше заложить заранее, чтобы модуль нормально встроился в текущий outbox/inbox-подход проекта.

## 7. Подключение данных каталога

Консультанту нужны:

1. Товары.
2. Категории.
3. Характеристики товаров.
4. Наличие товара.
5. Цена.
6. Бренд.

Возможны два подхода.

Первый подход для MVP: консультант вызывает query-сервисы каталога через интерфейс.

Второй подход для масштабирования: консультант хранит свой read-model товаров и обновляет его через события `ProductCreatedEvent`, `ProductPriceChangedEvent`, `ProductDeletedEvent`, `StockChangedEvent`.

Рекомендуемый путь:

1. Сначала реализовать MVP через query-интерфейс.
2. Затем добавить read-model `ConsultingProductEntity`.

## 8. Rule Engine

Создать доменные сервисы:

```text
CompatibilityChecker
RecommendationService
ConsultationExplainer
CartAdvisor
```

Логика проверки:

1. Получить товары.
2. Разложить товары по категориям.
3. Найти применимые правила.
4. Сравнить характеристики.
5. Сформировать результат:
   - `Ok`;
   - `Warning`;
   - `Error`;
   - `Unknown`.
6. Добавить объяснение.
7. Найти альтернативы или аксессуары.

Результат должен быть объяснимым, а не просто `true` или `false`.

## 9. CQRS-сценарии

Полный набор команд и запросов:

```text
CheckProductCompatibilityQuery
CheckCartCompatibilityQuery
GetRecommendationsQuery
StartConsultationCommand
SendConsultationAnswerCommand
GetConsultationResultQuery
CreateCompatibilityRuleCommand
UpdateCompatibilityRuleCommand
DeleteCompatibilityRuleCommand
```

Для MVP достаточно:

```text
CheckCartCompatibilityQuery
GetProductRecommendationsQuery
CreateCompatibilityRuleCommand
GetCompatibilityRulesQuery
```

## 10. API

Добавить `ConsultingController`.

Эндпоинты:

```http
POST /Consulting/cart/check
GET  /Consulting/products/{productId}/recommendations
POST /Consulting/products/{productId}/compatibility
GET  /Consulting/rules
POST /Consulting/rules
PUT  /Consulting/rules/{ruleId}
DELETE /Consulting/rules/{ruleId}
```

Пример ответа проверки корзины:

```json
{
  "status": "Warning",
  "issues": [
    {
      "severity": "Error",
      "message": "Процессор не совместим с выбранной материнской платой",
      "sourceProductId": "...",
      "targetProductId": "...",
      "reason": "Socket LGA1700 не совпадает с AM4"
    }
  ],
  "recommendations": [
    {
      "productId": "...",
      "reason": "Совместимая альтернатива с подходящим socket"
    }
  ]
}
```

## 11. Интеграция с корзиной

На странице корзины добавить автоматическую проверку:

1. Пользователь открывает корзину.
2. Frontend вызывает `/Consulting/cart/check`.
3. Если есть проблемы, показывается блок предупреждений.
4. Если есть рекомендации, показываются карточки товаров.
5. При checkout, если есть `Error`, показывать подтверждение или блокировать оформление в зависимости от выбранной бизнес-логики.

Для MVP лучше не блокировать checkout жестко, а показывать заметное предупреждение. Это проще объяснить как помощь консультанта, а не как запрет системы.

## 12. Интеграция с карточкой товара

На `ProductDetailsView` добавить:

1. Блок "Совместимость".
2. Блок "Подойдет, если...".
3. Блок "Часто покупают вместе".
4. Блок "Похожие совместимые товары".

Пример текста:

```text
Совместимость
Этот SSD подойдет для устройств с интерфейсом M.2 NVMe.

Рекомендации
Добавьте комплект для установки, если в ноутбуке нет крепления.
```

## 13. UI для менеджера

В админке добавить вкладку "Консультант".

В ней должны быть:

1. Список правил.
2. Создание правила.
3. Редактирование условия.
4. Выбор категорий.
5. Выбор характеристик.
6. Текст объяснения для покупателя.
7. Включение и выключение правила.
8. Тест правила на выбранных товарах.

Это важно для продуктовой ценности: экспертные знания магазина становятся управляемой базой правил.

## 14. Frontend API

В `frontend/src/api/types.ts` добавить:

```text
ConsultationResult
CompatibilityIssue
ProductRecommendation
CompatibilityRule
CompatibilityRulePayload
```

В `frontend/src/api/client.ts` добавить методы:

```ts
checkCartCompatibility()
productRecommendations(productId: string)
compatibilityRules()
createCompatibilityRule(payload)
updateCompatibilityRule(ruleId, payload)
deleteCompatibilityRule(ruleId)
```

## 15. Frontend-хуки

Создать:

```text
frontend/src/hooks/useConsultingData.ts
frontend/src/hooks/useCartConsultation.ts
frontend/src/hooks/useProductRecommendations.ts
```

Задачи хуков:

1. Загружать проверку корзины.
2. Обновлять проверку после изменения количества.
3. Загружать рекомендации на карточке товара.
4. Обрабатывать loading/error-состояния.

## 16. Backend-тесты

Тесты домена:

1. Совпадающие характеристики дают `Ok`.
2. Несовместимые характеристики дают `Error`.
3. Отсутствующие характеристики дают `Unknown`.
4. Правило с severity `Warning` не блокирует результат.

Тесты application-слоя:

1. `CheckCartCompatibilityQueryHandler`.
2. `GetProductRecommendationsQueryHandler`.
3. Создание и редактирование правил.

API-тесты:

1. Доступность endpoint.
2. Авторизация менеджерских endpoint.
3. Корректный JSON-ответ.

Архитектурные тесты:

1. `Store.Consulting` не нарушает изоляцию слоев.
2. Domain не зависит от Infrastructure.
3. API не содержит бизнес-логики.

## 17. Demo-данные

В `SeedDemoElectronicsAsync` или отдельном seed-расширении добавить категории:

```text
процессоры
материнские платы
RAM
SSD
блоки питания
ноутбуки
аксессуары
```

Добавить характеристики:

```text
socket
memoryType
formFactor
interface
powerConsumption
recommendedPsuPower
deviceModel
connectorType
```

Добавить правила:

1. CPU socket должен совпадать с motherboard socket.
2. RAM type должен совпадать с motherboard memory type.
3. SSD interface должен поддерживаться устройством.
4. Мощность БП должна покрывать потребление сборки.
5. Аксессуары рекомендуются по категории и характеристикам.

## 18. Этапы реализации

### Этап 1. Backend MVP

1. Создать проекты `Store.Consulting` и `Store.Consulting.Contracts`.
2. Добавить `ConsultingDbContext`.
3. Добавить сущность `CompatibilityRule`.
4. Добавить миграцию.
5. Добавить `CompatibilityChecker`.
6. Добавить `CheckCartCompatibilityQuery`.
7. Добавить `ConsultingController`.
8. Подключить модуль в `Program.cs`.

### Этап 2. Frontend MVP

1. Добавить типы.
2. Добавить API-методы.
3. Добавить `useCartConsultation`.
4. Добавить блок проверки в корзину.
5. Добавить рекомендации в карточку товара.

### Этап 3. Админка

1. Добавить вкладку "Консультант".
2. Добавить список правил.
3. Добавить форму создания и редактирования.
4. Добавить тестирование правила на двух товарах.

### Этап 4. Расширение рекомендаций

1. Добавить аксессуары.
2. Добавить альтернативы.
3. Учитывать наличие на складе.
4. Учитывать цену.
5. Учитывать категорию и бренд.

### Этап 5. События и read-model

1. Добавить read-model товаров в `Store.Consulting`.
2. Подписаться на события каталога.
3. Подписаться на события склада.
4. Обновлять данные консультанта асинхронно через outbox/inbox.

## 19. Критерии готовности

Модуль можно считать реализованным, если:

1. В корзине показываются предупреждения о несовместимости.
2. На карточке товара есть рекомендации.
3. Менеджер может создавать и менять правила.
4. Проверка объясняет причину результата.
5. Есть тесты доменной логики.
6. Модуль подключен как отдельный bounded context.
7. Checkout не ломается при ошибке консультанта.
8. При отсутствии данных система честно показывает `Unknown`, а не выдумывает совместимость.

## 20. Рекомендуемый MVP-объем

Самая удачная первая версия:

```text
1. Отдельный Store.Consulting.
2. Правила совместимости по характеристикам.
3. Проверка корзины.
4. Рекомендации на карточке товара.
5. Админское управление правилами.
6. 5-7 demo-правил для электроники.
```

LLM-чат лучше добавить позже как надстройку. Сначала правила, объяснения и рекомендации должны работать надежно. Так консультант будет не "магическим окном с текстом", а настоящей экспертной системой внутри магазина.
