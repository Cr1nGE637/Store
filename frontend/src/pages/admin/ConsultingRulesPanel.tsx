import { Beaker, Bot, HelpCircle, Pencil, Plus, RotateCcw, Save, Trash2 } from "lucide-react";
import { FormEvent, useMemo, useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import styled from "styled-components";
import { api, session } from "../../api/client";
import { queryKeys } from "../../api/queryKeys";
import type { Category, CompatibilityRule, CompatibilityRulePayload, ConsultationResult, Product } from "../../api/types";
import { GhostButton, PrimaryButton } from "../../components/ui/buttons";
import { FormError, Panel, PanelHeading, SoftBadge } from "../../components/ui/common";
import { specificationOptions } from "../../utils/specifications";
import { FormGrid } from "./admin.styles";

const operators = ["Equals", "NotEquals", "In", "GreaterThanOrEqual", "LessThanOrEqual"];
const severities = ["Error", "Warning", "Unknown", "Ok"];
const recommendationTypes = ["Alternative", "Accessory", "RequiredPart"];
const messageTokens = ["{sourceProduct}", "{targetProduct}", "{sourceValue}", "{targetValue}"];
const operatorLabels: Record<string, string> = {
  Equals: "Равно",
  NotEquals: "Не равно",
  In: "Входит в список",
  GreaterThanOrEqual: "Больше или равно",
  LessThanOrEqual: "Меньше или равно"
};
const severityLabels: Record<string, string> = {
  Error: "Ошибка",
  Warning: "Предупреждение",
  Unknown: "Неизвестно",
  Ok: "Успешно"
};
const recommendationLabels: Record<string, string> = {
  Alternative: "Альтернатива",
  Accessory: "Аксессуар",
  RequiredPart: "Обязательная деталь"
};
const defaultCategoryCodes = [
  "Smartphones",
  "Laptops",
  "Tablets",
  "Televisions",
  "Monitors",
  "Headphones",
  "Chargers",
  "CablesAdapters",
  "SmartWatches",
  "GameConsoles",
  "NetworkEquipment",
  "Accessories",
  "Peripherals",
  "Processors",
  "Motherboards",
  "RAM",
  "SSD",
  "GraphicsCards",
  "PowerSupplies"
];

const emptyRule: CompatibilityRulePayload = {
  code: "",
  name: "",
  sourceCategoryCode: "Processors",
  targetCategoryCode: "Motherboards",
  sourceSpecificationKey: "socket",
  targetSpecificationKey: "socket",
  operator: "Equals",
  expectedValue: "",
  severity: "Error",
  messageTemplate: "{sourceProduct} не подходит к {targetProduct}: {sourceValue} != {targetValue}.",
  recommendationType: "Alternative",
  isActive: true
};

type ConsultingRulesPanelProps = {
  categories: Category[];
  products: Product[];
  embedded?: boolean;
};

export function ConsultingRulesPanel({ categories, products, embedded = false }: ConsultingRulesPanelProps) {
  const [payload, setPayload] = useState<CompatibilityRulePayload>(emptyRule);
  const [editingRuleId, setEditingRuleId] = useState<string | null>(null);
  const [sourceProductId, setSourceProductId] = useState("");
  const [targetProductId, setTargetProductId] = useState("");
  const [testResult, setTestResult] = useState<ConsultationResult | null>(null);
  const [error, setError] = useState<string | null>(null);
  const queryClient = useQueryClient();
  const token = session.get()?.token;

  const rulesQuery = useQuery({
    queryKey: queryKeys.compatibilityRules(token),
    queryFn: api.compatibilityRules,
    staleTime: 20_000
  });

  const categoryCodes = useMemo(() => {
    const codes = categories.map((category) => category.categoryCode).filter(Boolean);
    return Array.from(new Set([...codes, ...defaultCategoryCodes]));
  }, [categories]);

  const saveRule = useMutation({
    mutationFn: (nextPayload: CompatibilityRulePayload) =>
      editingRuleId
        ? api.updateCompatibilityRule(editingRuleId, nextPayload)
        : api.createCompatibilityRule(nextPayload),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: queryKeys.compatibilityRulesRoot });
    }
  });

  const deleteRule = useMutation({
    mutationFn: api.deleteCompatibilityRule,
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: queryKeys.compatibilityRulesRoot });
    }
  });

  const testRule = useMutation({
    mutationFn: (nextPayload: CompatibilityRulePayload) =>
      api.testCompatibilityRule(sourceProductId, targetProductId, nextPayload)
  });

  function normalizePayload(): CompatibilityRulePayload {
    return {
      ...payload,
      code: payload.code.trim(),
      name: payload.name.trim(),
      sourceCategoryCode: payload.sourceCategoryCode.trim(),
      targetCategoryCode: payload.targetCategoryCode.trim(),
      sourceSpecificationKey: payload.sourceSpecificationKey.trim(),
      targetSpecificationKey: payload.targetSpecificationKey.trim(),
      expectedValue: payload.expectedValue?.trim() || null,
      messageTemplate: payload.messageTemplate.trim(),
      recommendationType: payload.recommendationType?.trim() || null
    };
  }

  async function submitRule(event: FormEvent) {
    event.preventDefault();
    setError(null);
    setTestResult(null);

    try {
      await saveRule.mutateAsync(normalizePayload());
      resetForm();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Не удалось сохранить правило");
    }
  }

  async function removeRule(ruleId: string) {
    setError(null);
    setTestResult(null);

    try {
      await deleteRule.mutateAsync(ruleId);
      if (editingRuleId === ruleId) resetForm();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Не удалось удалить правило");
    }
  }

  async function runTest() {
    setError(null);
    setTestResult(null);

    try {
      const result = await testRule.mutateAsync(normalizePayload());
      setTestResult(result);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Не удалось проверить правило");
    }
  }

  function editRule(rule: CompatibilityRule) {
    setError(null);
    setTestResult(null);
    setEditingRuleId(rule.id);
    setPayload({
      code: rule.code,
      name: rule.name,
      sourceCategoryCode: rule.sourceCategoryCode,
      targetCategoryCode: rule.targetCategoryCode,
      sourceSpecificationKey: rule.sourceSpecificationKey,
      targetSpecificationKey: rule.targetSpecificationKey,
      operator: rule.operator,
      expectedValue: rule.expectedValue ?? "",
      severity: rule.severity,
      messageTemplate: rule.messageTemplate,
      recommendationType: rule.recommendationType ?? "",
      isActive: rule.isActive
    });
  }

  function resetForm() {
    setEditingRuleId(null);
    setPayload(emptyRule);
  }

  function applyPreset(preset: "cpuBoard" | "boardRam" | "psuGpu" | "phoneCharger" | "chargerPower" | "phoneAccessory" | "laptopCable" | "cableMonitor") {
    const presets: Record<typeof preset, CompatibilityRulePayload> = {
      cpuBoard: {
        ...emptyRule,
        code: "cpu_motherboard_socket",
        name: "CPU and motherboard socket",
        sourceCategoryCode: "Processors",
        targetCategoryCode: "Motherboards",
        sourceSpecificationKey: "socket",
        targetSpecificationKey: "socket",
        operator: "Equals",
        severity: "Error",
        messageTemplate: "{sourceProduct} socket {sourceValue} does not match {targetProduct} socket {targetValue}.",
        recommendationType: "Alternative"
      },
      boardRam: {
        ...emptyRule,
        code: "motherboard_ram_memory_type",
        name: "Motherboard and RAM memory type",
        sourceCategoryCode: "Motherboards",
        targetCategoryCode: "RAM",
        sourceSpecificationKey: "memoryType",
        targetSpecificationKey: "memoryType",
        operator: "Equals",
        severity: "Error",
        messageTemplate: "{sourceProduct} requires {sourceValue} memory, but {targetProduct} is {targetValue}.",
        recommendationType: "Alternative"
      },
      psuGpu: {
        ...emptyRule,
        code: "psu_gpu_power",
        name: "PSU capacity and GPU recommendation",
        sourceCategoryCode: "PowerSupplies",
        targetCategoryCode: "GraphicsCards",
        sourceSpecificationKey: "powerConsumptionWatts",
        targetSpecificationKey: "recommendedPsuWatts",
        operator: "GreaterThanOrEqual",
        severity: "Error",
        messageTemplate: "{sourceProduct} capacity {sourceValue}W is below {targetProduct} recommended PSU {targetValue}W.",
        recommendationType: "RequiredPart"
      },
      phoneCharger: {
        ...emptyRule,
        code: "smartphone_charger_connector",
        name: "Smartphone and charger connector",
        sourceCategoryCode: "Smartphones",
        targetCategoryCode: "Chargers",
        sourceSpecificationKey: "connectorType",
        targetSpecificationKey: "connectorType",
        operator: "Equals",
        severity: "Error",
        messageTemplate: "{targetProduct} connector {targetValue} does not match {sourceProduct} connector {sourceValue}.",
        recommendationType: ""
      },
      chargerPower: {
        ...emptyRule,
        code: "smartphone_charger_power",
        name: "Smartphone charger power",
        sourceCategoryCode: "Chargers",
        targetCategoryCode: "Smartphones",
        sourceSpecificationKey: "powerWatts",
        targetSpecificationKey: "requiredChargerWatts",
        operator: "GreaterThanOrEqual",
        severity: "Warning",
        messageTemplate: "{sourceProduct} power {sourceValue}W is below {targetProduct} recommended charger power {targetValue}W.",
        recommendationType: "Accessory"
      },
      phoneAccessory: {
        ...emptyRule,
        code: "smartphone_accessory_model",
        name: "Smartphone accessory device model",
        sourceCategoryCode: "Smartphones",
        targetCategoryCode: "Accessories",
        sourceSpecificationKey: "deviceModel",
        targetSpecificationKey: "compatibleDeviceModel",
        operator: "In",
        severity: "Error",
        messageTemplate: "{targetProduct} is intended for {targetValue}, not for {sourceProduct} ({sourceValue}).",
        recommendationType: "Accessory"
      },
      laptopCable: {
        ...emptyRule,
        code: "laptop_cable_input_connector",
        name: "Laptop and cable input connector",
        sourceCategoryCode: "Laptops",
        targetCategoryCode: "CablesAdapters",
        sourceSpecificationKey: "connectorType",
        targetSpecificationKey: "cableInputType",
        operator: "Equals",
        severity: "Error",
        messageTemplate: "{targetProduct} input {targetValue} does not match {sourceProduct} connector {sourceValue}.",
        recommendationType: "Accessory"
      },
      cableMonitor: {
        ...emptyRule,
        code: "cable_monitor_output_connector",
        name: "Cable output and monitor connector",
        sourceCategoryCode: "CablesAdapters",
        targetCategoryCode: "Monitors",
        sourceSpecificationKey: "cableOutputType",
        targetSpecificationKey: "connectorType",
        operator: "Equals",
        severity: "Error",
        messageTemplate: "{sourceProduct} output {sourceValue} does not match {targetProduct} connector {targetValue}.",
        recommendationType: "Accessory"
      }
    };

    setError(null);
    setTestResult(null);
    setEditingRuleId(null);
    setPayload(presets[preset]);
  }

  const rules = rulesQuery.data ?? [];
  const isBusy = saveRule.isPending || deleteRule.isPending || testRule.isPending;
  const Shell = embedded ? RulesContent : RulesPanel;

  return (
    <Shell>
      <PanelHeading>
        <div>
          <h2>Консультант</h2>
          <p>Правила совместимости, рекомендации и ручная проверка пары товаров</p>
        </div>
        <SoftBadge>{rulesQuery.isLoading ? "Загрузка" : `${rules.length} правил`}</SoftBadge>
      </PanelHeading>

      <RuleHelp>
        <HelpCircle size={18} />
        <div>
          <strong>Как читать правило</strong>
          <span>Источник сравнивается с целью. Если ожидаемое значение пустое, консультант берет значение из характеристики целевого товара.</span>
          <TokenRow>
            {messageTokens.map((token) => <code key={token}>{token}</code>)}
          </TokenRow>
        </div>
      </RuleHelp>

      <PresetRow>
        <GhostButton type="button" onClick={() => applyPreset("cpuBoard")}>CPU + плата</GhostButton>
        <GhostButton type="button" onClick={() => applyPreset("boardRam")}>Плата + RAM</GhostButton>
        <GhostButton type="button" onClick={() => applyPreset("psuGpu")}>БП + GPU</GhostButton>
        <GhostButton type="button" onClick={() => applyPreset("phoneCharger")}>Смартфон + зарядка</GhostButton>
        <GhostButton type="button" onClick={() => applyPreset("chargerPower")}>Мощность зарядки</GhostButton>
        <GhostButton type="button" onClick={() => applyPreset("phoneAccessory")}>Аксессуар + модель</GhostButton>
        <GhostButton type="button" onClick={() => applyPreset("laptopCable")}>Ноутбук + кабель</GhostButton>
        <GhostButton type="button" onClick={() => applyPreset("cableMonitor")}>Кабель + монитор</GhostButton>
      </PresetRow>

      {rulesQuery.error && <FormError>{rulesErrorMessage(rulesQuery.error)}</FormError>}
      {error && <FormError>{error}</FormError>}

      <RuleForm onSubmit={submitRule}>
        <FormGrid>
          <FieldGroup>
            <span>Код правила</span>
            <small>Короткий технический идентификатор без пробелов. По нему правило проще искать в логах и списке.</small>
            <input value={payload.code} onChange={(event) => setPayload({ ...payload, code: event.target.value })} placeholder="cpu_motherboard_socket" />
          </FieldGroup>
          <FieldGroup>
            <span>Название правила</span>
            <small>Понятное имя для менеджера: что именно проверяет консультант.</small>
            <input value={payload.name} onChange={(event) => setPayload({ ...payload, name: event.target.value })} placeholder="Сокет процессора и платы" />
          </FieldGroup>
        </FormGrid>

        <FormGrid>
          <FieldGroup>
            <span>Категория источника</span>
            <small>Товар слева в сравнении. Например, процессор, у которого берем socket.</small>
            <select value={payload.sourceCategoryCode} onChange={(event) => setPayload({ ...payload, sourceCategoryCode: event.target.value })}>
              {categoryCodes.map((code) => <option key={code} value={code}>{code}</option>)}
            </select>
          </FieldGroup>
          <FieldGroup>
            <span>Категория цели</span>
            <small>Товар справа в сравнении. Например, материнская плата, с которой сверяем socket.</small>
            <select value={payload.targetCategoryCode} onChange={(event) => setPayload({ ...payload, targetCategoryCode: event.target.value })}>
              {categoryCodes.map((code) => <option key={code} value={code}>{code}</option>)}
            </select>
          </FieldGroup>
        </FormGrid>

        <FormGrid>
          <FieldGroup>
            <span>Характеристика источника</span>
            <small>Поле товара-источника, значение которого консультант будет сравнивать.</small>
            <select value={payload.sourceSpecificationKey} onChange={(event) => setPayload({ ...payload, sourceSpecificationKey: event.target.value })}>
              {specificationOptions.map((option) => <option key={option.key} value={option.key}>{option.label}</option>)}
            </select>
          </FieldGroup>
          <FieldGroup>
            <span>Характеристика цели</span>
            <small>Поле товара-цели. Если ожидаемое значение пустое, сравнение идет именно с этим полем.</small>
            <select value={payload.targetSpecificationKey} onChange={(event) => setPayload({ ...payload, targetSpecificationKey: event.target.value })}>
              {specificationOptions.map((option) => <option key={option.key} value={option.key}>{option.label}</option>)}
            </select>
          </FieldGroup>
        </FormGrid>

        <FormGrid>
          <FieldGroup>
            <span>Как сравнивать</span>
            <small>Условие, по которому консультант решит, совместимы товары или нет.</small>
            <select value={payload.operator} onChange={(event) => setPayload({ ...payload, operator: event.target.value })}>
              {operators.map((operator) => <option key={operator} value={operator}>{operatorLabels[operator] ?? operator}</option>)}
            </select>
          </FieldGroup>
          <FieldGroup>
            <span>Ожидаемое значение</span>
            <small>Заполняется, когда нужно сравнить с фиксированным значением, а не с характеристикой цели.</small>
            <input value={payload.expectedValue ?? ""} onChange={(event) => setPayload({ ...payload, expectedValue: event.target.value })} placeholder="Например: AM5 или 750" />
          </FieldGroup>
        </FormGrid>
        <FieldHint>{operatorHint(payload.operator)}</FieldHint>

        <FormGrid>
          <FieldGroup>
            <span>Что показать покупателю</span>
            <small>Уровень результата: ошибка блокирует совместимость, предупреждение только подсказывает риск.</small>
            <select value={payload.severity} onChange={(event) => setPayload({ ...payload, severity: event.target.value })}>
              {severities.map((severity) => <option key={severity} value={severity}>{severityLabels[severity] ?? severity}</option>)}
            </select>
          </FieldGroup>
          <FieldGroup>
            <span>Тип рекомендации</span>
            <small>Какие товары можно предложить, если правило нашло проблему.</small>
            <select value={payload.recommendationType ?? ""} onChange={(event) => setPayload({ ...payload, recommendationType: event.target.value })}>
              <option value="">Без рекомендации</option>
              {recommendationTypes.map((type) => <option key={type} value={type}>{recommendationLabels[type] ?? type}</option>)}
            </select>
          </FieldGroup>
        </FormGrid>

        <FieldGroup>
          <span>Сообщение для покупателя</span>
          <small>Текст, который консультант покажет в корзине или проверке совместимости.</small>
          <textarea value={payload.messageTemplate} onChange={(event) => setPayload({ ...payload, messageTemplate: event.target.value })} placeholder="Сообщение для покупателя" />
        </FieldGroup>
        <FieldHint>В сообщении можно использовать токены из подсказки выше, чтобы показать покупателю конкретные товары и значения.</FieldHint>

        <ActiveToggle>
          <input checked={payload.isActive} onChange={(event) => setPayload({ ...payload, isActive: event.target.checked })} type="checkbox" />
          <span>
            Правило активно
            <small>Если выключить, правило сохранится, но консультант не будет учитывать его в проверках.</small>
          </span>
        </ActiveToggle>

        <ActionRow>
          <PrimaryButton type="submit" disabled={isBusy}>
            {editingRuleId ? <Save size={18} /> : <Plus size={18} />}
            {editingRuleId ? "Сохранить правило" : "Создать правило"}
          </PrimaryButton>
          <GhostButton type="button" onClick={resetForm} disabled={isBusy}>
            <RotateCcw size={16} /> Сбросить
          </GhostButton>
        </ActionRow>
      </RuleForm>

      <TestBox>
        <strong>Тест правила</strong>
        <FormGrid>
          <FieldGroup>
            <span>Товар-источник для теста</span>
            <small>Должен относиться к категории источника в правиле.</small>
            <select value={sourceProductId} onChange={(event) => setSourceProductId(event.target.value)}>
              <option value="">Первый товар</option>
              {products.map((product) => <option key={product.productId} value={product.productId}>{product.productName}</option>)}
            </select>
          </FieldGroup>
          <FieldGroup>
            <span>Товар-цель для теста</span>
            <small>Должен относиться к категории цели в правиле.</small>
            <select value={targetProductId} onChange={(event) => setTargetProductId(event.target.value)}>
              <option value="">Второй товар</option>
              {products.map((product) => <option key={product.productId} value={product.productId}>{product.productName}</option>)}
            </select>
          </FieldGroup>
        </FormGrid>
        <GhostButton type="button" onClick={runTest} disabled={!sourceProductId || !targetProductId || isBusy}>
          <Beaker size={16} /> Проверить пару
        </GhostButton>
        {testResult && (
          <TestResult>
            <SoftBadge>{testResult.status}</SoftBadge>
            {testResult.findings.length === 0 ? (
              <span>Замечаний нет</span>
            ) : (
              testResult.findings.map((finding) => <span key={`${finding.ruleCode}-${finding.message}`}>{finding.message}</span>)
            )}
          </TestResult>
        )}
      </TestBox>

      <RulesList>
        {rules.map((rule) => (
          <RuleRow key={rule.id}>
            <Bot size={18} />
            <div>
              <strong>{rule.name}</strong>
              <span>{`${rule.sourceCategoryCode} -> ${rule.targetCategoryCode} / ${rule.sourceSpecificationKey}:${rule.targetSpecificationKey}`}</span>
              <small>{rule.messageTemplate}</small>
            </div>
            <SoftBadge>{rule.isActive ? rule.severity : "Off"}</SoftBadge>
            <RowActions>
              <GhostButton type="button" onClick={() => editRule(rule)} disabled={isBusy}>
                <Pencil size={16} />
              </GhostButton>
              <GhostButton type="button" onClick={() => void removeRule(rule.id)} disabled={isBusy}>
                <Trash2 size={16} />
              </GhostButton>
            </RowActions>
          </RuleRow>
        ))}
      </RulesList>
    </Shell>
  );
}

function operatorHint(operator: string) {
  if (operator === "Equals") return "Equals: значения должны совпадать, например socket AM4 у процессора и платы.";
  if (operator === "NotEquals") return "NotEquals: правило срабатывает, когда значения отличаются.";
  if (operator === "In") return "In: значение источника должно входить в список ожидаемых значений через запятую.";
  if (operator === "GreaterThanOrEqual") return "GreaterThanOrEqual: значение источника должно быть не меньше целевого или ожидаемого.";
  if (operator === "LessThanOrEqual") return "LessThanOrEqual: значение источника должно быть не больше целевого или ожидаемого.";
  return "Выберите оператор сравнения характеристик.";
}

function rulesErrorMessage(error: unknown) {
  const message = error instanceof Error ? error.message : "Не удалось загрузить правила";
  if (message.includes("Сессия истекла") || message.includes("401")) {
    return `${message} Демо-доступ: manager@gmail.com / manager123.`;
  }

  if (message.includes("роль менеджера") || message.includes("403")) {
    return "Правила консультанта доступны только менеджеру. Войдите как manager@gmail.com.";
  }

  return message;
}

const RulesPanel = styled(Panel)`
  display: grid;
  gap: 14px;
  align-content: start;
  padding: 16px;
`;

const RulesContent = styled.div`
  display: grid;
  gap: 14px;
  align-content: start;
`;

const RuleForm = styled.form`
  display: grid;
  gap: 10px;
`;

const RuleHelp = styled.div`
  display: grid;
  grid-template-columns: auto minmax(0, 1fr);
  gap: 10px;
  border: 1px solid #d8e0de;
  border-radius: 8px;
  padding: 12px;
  color: #3f4d49;
  background: #f4f7f6;

  strong,
  span {
    display: block;
  }

  span {
    margin-top: 4px;
    line-height: 1.4;
  }
`;

const TokenRow = styled.div`
  display: flex;
  flex-wrap: wrap;
  gap: 6px;
  margin-top: 10px;

  code {
    border-radius: 6px;
    padding: 4px 7px;
    color: #16684f;
    background: #e6f6ef;
    font-size: 12px;
    font-weight: 700;
  }
`;

const PresetRow = styled.div`
  display: grid;
  grid-template-columns: repeat(3, minmax(0, 1fr));
  gap: 8px;

  @media (max-width: 760px) {
    grid-template-columns: 1fr;
  }
`;

const FieldHint = styled.small`
  color: #64736e;
  line-height: 1.35;
`;

const FieldGroup = styled.label`
  display: grid;
  gap: 6px;
  min-width: 0;

  > span {
    color: #172026;
    font-size: 14px;
    font-weight: 800;
    line-height: 1.25;
  }

  > small {
    min-height: 34px;
    color: #64736e;
    font-size: 12px;
    line-height: 1.35;
  }

  input,
  select,
  textarea {
    width: 100%;
  }
`;

const ActiveToggle = styled.label`
  display: inline-flex;
  align-items: flex-start;
  gap: 8px;
  color: #3f4d49;
  font-weight: 700;

  input {
    margin-top: 3px;
    width: auto;
  }

  span {
    display: grid;
    gap: 3px;
  }

  small {
    color: #64736e;
    font-size: 12px;
    font-weight: 500;
    line-height: 1.35;
  }
`;

const ActionRow = styled.div`
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
`;

const TestBox = styled.div`
  display: grid;
  gap: 10px;
  border: 1px solid #e4ebe8;
  border-radius: 8px;
  padding: 12px;
  background: #f8faf9;
`;

const TestResult = styled.div`
  display: grid;
  gap: 6px;
  color: #3f4d49;
  line-height: 1.35;
`;

const RulesList = styled.div`
  display: grid;
  gap: 10px;
`;

const RuleRow = styled.div`
  display: grid;
  grid-template-columns: auto minmax(0, 1fr) auto auto;
  gap: 10px;
  align-items: start;
  border-top: 1px solid #e4ebe8;
  padding-top: 12px;

  strong,
  span,
  small {
    display: block;
  }

  span,
  small {
    margin-top: 4px;
    color: #64736e;
    line-height: 1.35;
  }

  @media (max-width: 760px) {
    grid-template-columns: 1fr;
  }
`;

const RowActions = styled.div`
  display: flex;
  gap: 6px;

  button {
    min-width: 40px;
    padding-inline: 10px;
  }
`;
