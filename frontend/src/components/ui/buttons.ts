import styled from "styled-components";

export const PrimaryButton = styled.button`
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  min-height: 40px;
  border: 0;
  border-radius: 8px;
  padding: 10px 14px;
  color: #ffffff;
  background: #238466;
  font-weight: 700;
  transition:
    background 0.16s ease,
    box-shadow 0.16s ease,
    transform 0.16s ease,
    opacity 0.16s ease;

  &:hover:not(:disabled) {
    background: #1c6d55;
    box-shadow: 0 6px 14px rgba(35, 132, 102, 0.22);
  }

  &:active:not(:disabled) {
    transform: translateY(1px);
  }

  &:focus-visible {
    outline: 3px solid rgba(35, 132, 102, 0.22);
    outline-offset: 2px;
  }

  &:disabled {
    cursor: not-allowed;
    opacity: 0.58;
  }
`;

export const GhostButton = styled.button`
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  min-height: 40px;
  border: 0;
  border-radius: 8px;
  padding: 9px 12px;
  color: #172026;
  background: #dfe8e5;
  font-weight: 700;
  transition:
    background 0.16s ease,
    box-shadow 0.16s ease,
    transform 0.16s ease,
    opacity 0.16s ease;

  &:hover:not(:disabled) {
    background: #d0ddd9;
  }

  &:active:not(:disabled) {
    transform: translateY(1px);
  }

  &:focus-visible {
    outline: 3px solid rgba(35, 132, 102, 0.18);
    outline-offset: 2px;
  }

  &:disabled {
    cursor: not-allowed;
    opacity: 0.58;
  }
`;

export const SidebarGhostButton = styled(GhostButton)`
  width: 100%;
  margin-top: 10px;
`;
