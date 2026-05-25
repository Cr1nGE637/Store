import styled from "styled-components";

export const EmptyState = styled.div`
  display: grid;
  min-height: 180px;
  place-items: center;
  border: 1px dashed #b7c6c2;
  border-radius: 8px;
  padding: 24px;
  color: #64736e;
  background: #ffffff;
  text-align: center;
  line-height: 1.45;
`;

export const FormError = styled.span`
  display: block;
  border: 1px solid #f0b8ae;
  border-radius: 8px;
  padding: 9px 10px;
  overflow-wrap: anywhere;
  color: #9a342b;
  background: #fff1ef;
  font-size: 13px;
`;

export const TwoColumn = styled.div`
  display: grid;
  grid-template-columns: minmax(0, 1.4fr) minmax(320px, 0.8fr);
  gap: 14px;

  @media (max-width: 760px) {
    grid-template-columns: 1fr;
  }
`;

export const CardHeading = styled.div`
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 12px;

  strong {
    display: block;
    font-size: 17px;
    line-height: 1.25;
  }

  span {
    color: #64736e;
    font-size: 14px;
  }

  @media (max-width: 760px) {
    flex-direction: column;
  }
`;

export const MetaRow = styled.div`
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: 8px;
  color: #64736e;
  font-size: 14px;

  span {
    border-radius: 999px;
    padding: 4px 8px;
    background: #eef2f3;
  }
`;

export const Panel = styled.section`
  border: 1px solid #d8e0de;
  border-radius: 8px;
  background: #ffffff;
  box-shadow: 0 1px 2px rgba(23, 32, 38, 0.04);
`;

export const PanelHeading = styled.div`
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 12px;
  padding-bottom: 12px;
  border-bottom: 1px solid #e4ebe8;

  h2 {
    margin: 0;
    font-size: 20px;
    line-height: 1.2;
  }

  p {
    margin: 5px 0 0;
    color: #64736e;
    font-size: 14px;
    line-height: 1.35;
  }
`;

export const SoftBadge = styled.span`
  display: inline-flex;
  align-items: center;
  min-height: 26px;
  border-radius: 999px;
  padding: 4px 9px;
  color: #3f4d49;
  background: #eef2f3;
  font-size: 13px;
  font-weight: 700;
  white-space: nowrap;
`;
