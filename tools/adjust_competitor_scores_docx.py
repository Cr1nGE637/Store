import os
import zipfile
import xml.etree.ElementTree as ET
from pathlib import Path


DOCX = next(Path("docs").glob("*.docx"))
TMP = DOCX.with_name(DOCX.stem + ".tmp.docx")
W = "http://schemas.openxmlformats.org/wordprocessingml/2006/main"
NS = {"w": W}

ET.register_namespace("w", W)
ET.register_namespace("r", "http://schemas.openxmlformats.org/officeDocument/2006/relationships")
ET.register_namespace("m", "http://schemas.openxmlformats.org/officeDocument/2006/math")
ET.register_namespace("wp", "http://schemas.openxmlformats.org/drawingml/2006/wordprocessingDrawing")


paragraph_replacements = {
    109: (
        "Результаты бенчмаркинга (таблица 2) демонстрируют, что зрелые платформы "
        "InSales, AdvantShop, Shop-Script/Webasyst, DIAFAN.CMS, Tilda и 1С-Битрикс "
        "сильнее StoreFit по готовности массового внедрения, поддержке и отработанным "
        "механизмам электронной коммерции. При этом они являются универсальными решениями "
        "и не закрывают специализированную задачу магазинов электроники: помощь покупателю "
        "в выборе совместимых товаров, работа с техническими характеристиками и снижение "
        "консультационной нагрузки менеджеров."
    ),
    110: (
        "Таким образом, конкурентная позиция StoreFit является нишевой: продукт не "
        "конкурирует с универсальными платформами по масштабу экосистемы, но предлагает "
        "более точное решение для конкретного сегмента — малых магазинов электроники. "
        "Итоговый балл StoreFit снижен с учетом стадии MVP: цифровой консультант и "
        "быстрое внедрение под клиента требуют дальнейшей доработки и валидации на пилотных "
        "магазинах."
    ),
}


table_replacements = {
    8: [
        ["Критерий / Вес", "StoreFit", "InSales", "AdvantShop", "Shop-Script / Webasyst", "DIAFAN.CMS", "Tilda", "1С-Битрикс"],
        ["Тип продукта", "Нишевая витрина электроники", "Платформа онлайн-бизнеса", "Облачная платформа магазина", "CMS / облако для магазина", "CMS и облачные тарифы", "Конструктор сайтов", "Коробочная CMS"],
        ["Шкала: 0 — нет; 1 — частично; 2 — реализовано", "", "", "", "", "", "", ""],
        ["Запуск интернет-магазина  [15%]", "1 — MVP, нужна настройка", "2 — есть", "2 — есть", "2 — есть", "2 — есть", "2 — есть", "1 — нужен интегратор"],
        ["Каталог, корзина, заказы  [15%]", "2 — есть", "2 — есть", "2 — есть", "2 — есть", "2 — есть", "1 — базово", "2 — есть"],
        ["Работа с остатками и складом  [15%]", "2 — есть", "2 — есть", "2 — есть", "2 — есть", "1 — частично", "1 — ограниченно", "2 — есть"],
    ],
    9: [
        ["Отраслевая специализация на электронике  [20%]", "2 — ядро продукта", "0 — универсальная платформа", "0 — универсальная платформа", "0 — универсальная CMS", "0 — универсальная CMS", "0 — универсальный конструктор", "0 — универсальная CMS"],
        ["Цифровой консультант по совместимости  [20%]", "1 — частично / в развитии", "0 — нет", "0 — нет", "0 — нет", "0 — нет", "0 — нет", "1 — возможна доработка"],
        ["Быстрое внедрение без команды разработки  [10%]", "1 — нужна настройка", "2 — SaaS", "2 — SaaS", "1 — нужна настройка", "1 — нужна настройка", "2 — SaaS", "0 — нужна разработка"],
        ["Поддержка и развитие платформы  [5%]", "1 — команда проекта", "2 — 24/7 поддержка", "2 — техподдержка", "2 — поддержка и плагины", "1 — поддержка по тарифу", "1 — поддержка платформы", "2 — партнерская сеть"],
        ["Взвешенный итог (макс. 2,00)", "1,50", "1,20", "1,20", "1,10", "0,90", "0,85", "1,05"],
        ["Цена входа / источник", "от 2 990 руб./мес.", "от 2 295 руб./мес.", "от 3 990 руб./мес.", "лицензия от 19 999 руб./год", "от 1 323 руб./мес.; коробка 39 000 руб.", "от 750 руб./мес.", "Малый бизнес 47 000 руб.; Бизнес 96 500 руб."],
    ],
}


def set_paragraph_text(paragraph, text):
    text_nodes = paragraph.findall(".//w:t", NS)
    if not text_nodes:
        return False
    text_nodes[0].text = text
    text_nodes[0].set("{http://www.w3.org/XML/1998/namespace}space", "preserve")
    for node in text_nodes[1:]:
        node.text = ""
    return True


with zipfile.ZipFile(DOCX, "r") as zin:
    root = ET.fromstring(zin.read("word/document.xml"))
    body = root.find(".//w:body", NS)
    paragraphs = body.findall("./w:p", NS)
    tables = body.findall(".//w:tbl", NS)

    for idx, text in paragraph_replacements.items():
        if idx < len(paragraphs):
            set_paragraph_text(paragraphs[idx], text)

    for table_idx, rows in table_replacements.items():
        if table_idx >= len(tables):
            continue
        table_rows = tables[table_idx].findall("./w:tr", NS)
        for row_idx, values in enumerate(rows):
            if row_idx >= len(table_rows):
                continue
            cells = table_rows[row_idx].findall("./w:tc", NS)
            for cell_idx, value in enumerate(values):
                if cell_idx >= len(cells):
                    continue
                cell_paragraphs = cells[cell_idx].findall(".//w:p", NS)
                if cell_paragraphs:
                    set_paragraph_text(cell_paragraphs[0], value)
                    for paragraph in cell_paragraphs[1:]:
                        for node in paragraph.findall(".//w:t", NS):
                            node.text = ""

    new_xml = ET.tostring(root, encoding="utf-8", xml_declaration=True)

    with zipfile.ZipFile(TMP, "w", zipfile.ZIP_DEFLATED) as zout:
        for item in zin.infolist():
            data = zin.read(item.filename)
            if item.filename == "word/document.xml":
                data = new_xml
            zout.writestr(item, data)

os.replace(TMP, DOCX)
print(f"updated: {DOCX}")
