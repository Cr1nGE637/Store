import { LogIn, UserPlus, UserRound } from "lucide-react";
import { FormEvent, useState } from "react";
import styled from "styled-components";
import { api, session } from "../../api/client";
import { PrimaryButton } from "../../components/ui/buttons";
import { FormError } from "../../components/ui/common";

type AuthState = ReturnType<typeof session.get>;

type AuthViewProps = {
  onLogin: (value: AuthState) => Promise<void> | void;
};

export function AuthView({ onLogin }: AuthViewProps) {
  const [mode, setMode] = useState<"login" | "register">("login");
  const [name, setName] = useState("");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  async function submit(event: FormEvent) {
    event.preventDefault();
    setError(null);
    setIsSubmitting(true);

    try {
      if (mode === "register") {
        await api.register(name, email, password);
      }

      await api.login(email, password);
      await onLogin(session.get());
    } catch (err) {
      setError(err instanceof Error ? err.message : "Auth error");
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <AuthLayout>
      <AuthCard>
        <IconMark>
          <UserRound size={28} />
        </IconMark>
        <HeaderBlock>
          <h2>{mode === "login" ? "Вход в аккаунт" : "Регистрация покупателя"}</h2>
          <p>{mode === "login" ? "Введите email и пароль для входа." : "Создайте аккаунт и продолжите оформление заказа."}</p>
        </HeaderBlock>

        <Segmented>
          <button type="button" data-selected={mode === "login"} onClick={() => setMode("login")}>
            <LogIn size={16} /> Вход
          </button>
          <button type="button" data-selected={mode === "register"} onClick={() => setMode("register")}>
            <UserPlus size={16} /> Регистрация
          </button>
        </Segmented>

        <Form onSubmit={submit}>
          {mode === "register" && <input value={name} onChange={(event) => setName(event.target.value)} placeholder="Имя" />}
          <input value={email} onChange={(event) => setEmail(event.target.value)} placeholder="Email" type="email" />
          <input value={password} onChange={(event) => setPassword(event.target.value)} placeholder="Пароль" type="password" />
          {error && <FormError>{error}</FormError>}
          <PrimaryButton disabled={isSubmitting} type="submit">
            <UserRound size={18} /> {isSubmitting ? "Подождите..." : mode === "login" ? "Войти" : "Создать аккаунт"}
          </PrimaryButton>
        </Form>
      </AuthCard>
    </AuthLayout>
  );
}

const AuthLayout = styled.section`
  display: grid;
  grid-template-columns: minmax(320px, 460px);
  justify-content: center;

  @media (max-width: 840px) {
    grid-template-columns: 1fr;
  }
`;

const AuthCard = styled.article`
  display: grid;
  gap: 16px;
  align-content: start;
  border: 1px solid #d8e0de;
  border-radius: 8px;
  padding: 20px;
  background: #ffffff;
`;

const IconMark = styled.div`
  display: grid;
  width: 54px;
  height: 54px;
  place-items: center;
  border-radius: 8px;
  color: #172026;
  background: #74d3ae;
`;

const HeaderBlock = styled.div`
  h2 {
    margin: 0;
    font-size: 24px;
  }

  p {
    margin: 8px 0 0;
    color: #64736e;
  }
`;

const Segmented = styled.div`
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 4px;
  padding: 4px;
  border-radius: 8px;
  background: #eef2f3;

  button {
    display: inline-flex;
    align-items: center;
    justify-content: center;
    gap: 8px;
    min-height: 38px;
    border: 0;
    border-radius: 6px;
    color: #3f4d49;
    background: transparent;
  }

  button[data-selected="true"] {
    color: #ffffff;
    background: #238466;
    font-weight: 700;
  }
`;

const Form = styled.form`
  display: grid;
  gap: 10px;
`;
