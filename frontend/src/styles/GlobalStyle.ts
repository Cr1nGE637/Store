import { createGlobalStyle } from "styled-components";

export const GlobalStyle = createGlobalStyle`
  :root {
    color-scheme: light;
    font-family:
      Inter, ui-sans-serif, system-ui, -apple-system, BlinkMacSystemFont, "Segoe UI",
      sans-serif;
    color: #172026;
    background: #eef2f3;
  }

  * {
    box-sizing: border-box;
  }

  body {
    margin: 0;
    min-width: 320px;
    min-height: 100vh;
  }

  button,
  input,
  select,
  textarea {
    font: inherit;
  }

  button {
    cursor: pointer;
  }

  button:disabled {
    cursor: not-allowed;
    opacity: 0.55;
  }

  input,
  select,
  textarea {
    width: 100%;
    border: 1px solid #cad5d2;
    border-radius: 8px;
    padding: 10px 12px;
    color: #172026;
    background: #ffffff;
    outline: none;
    transition:
      border-color 0.16s ease,
      box-shadow 0.16s ease,
      background 0.16s ease;
  }

  textarea {
    min-height: 92px;
    resize: vertical;
  }

  input:focus,
  select:focus,
  textarea:focus {
    border-color: #238466;
    box-shadow: 0 0 0 3px rgba(35, 132, 102, 0.12);
  }

  input:disabled,
  select:disabled,
  textarea:disabled {
    cursor: not-allowed;
    color: #71817d;
    background: #f4f7f6;
  }
`;
