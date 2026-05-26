import styled from "styled-components";
import { Panel } from "../../components/ui/common";

export const AdminForm = styled(Panel).attrs({ as: "form" })`
  display: grid;
  gap: 12px;
  align-content: start;
  padding: 16px;

  h2 {
    margin: 0;
    padding-bottom: 12px;
    border-bottom: 1px solid #e4ebe8;
    font-size: 20px;
    line-height: 1.2;
  }

  small {
    color: #64736e;
    line-height: 1.35;
  }
`;

export const AdminInlineForm = styled.form`
  display: grid;
  gap: 12px;
  align-content: start;
`;

export const FormGrid = styled.div`
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 10px;

  @media (max-width: 760px) {
    grid-template-columns: 1fr;
  }
`;
