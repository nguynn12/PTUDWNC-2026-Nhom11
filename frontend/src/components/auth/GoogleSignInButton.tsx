"use client";

import Script from "next/script";
import { useEffect, useRef, useState } from "react";
import styles from "@/styles/auth.module.css";

interface GoogleId {
  initialize: (config: {
    client_id: string;
    nonce?: string;
    callback: (response: { credential: string }) => void;
  }) => void;
  renderButton: (parent: HTMLElement, options: Record<string, unknown>) => void;
}

declare global {
  interface Window {
    google?: { accounts: { id: GoogleId } };
  }
}

interface GoogleSignInButtonProps {
  /** Nhận id_token (JWT bắt đầu bằng eyJ) và nonce đã gắn vào token. Phải là hàm ổn định (useCallback). */
  onCredential: (idToken: string, nonce: string) => void;
}

export function GoogleSignInButton({ onCredential }: GoogleSignInButtonProps) {
  const slot = useRef<HTMLDivElement>(null);
  const [ready, setReady] = useState(false);
  // FR-AUTH-003: nonce ngẫu nhiên cho mỗi lần mở trang, backend đối chiếu với claim "nonce" trong ID token.
  const [nonce] = useState(() => crypto.randomUUID());
  const clientId = process.env.NEXT_PUBLIC_GOOGLE_CLIENT_ID;

  useEffect(() => {
    if (!ready || !clientId || !slot.current || !window.google) return;
    window.google.accounts.id.initialize({
      client_id: clientId,
      nonce,
      callback: (response) => onCredential(response.credential, nonce),
    });
    window.google.accounts.id.renderButton(slot.current, {
      theme: "outline",
      size: "large",
      text: "continue_with",
      width: 356,
    });
  }, [ready, clientId, nonce, onCredential]);

  if (!clientId) {
    return <p className={styles.hint}>Chưa cấu hình NEXT_PUBLIC_GOOGLE_CLIENT_ID nên chưa dùng được đăng nhập Google.</p>;
  }

  return (
    <>
      <Script src="https://accounts.google.com/gsi/client" strategy="afterInteractive" onReady={() => setReady(true)} />
      <div ref={slot} className={styles.googleSlot} />
    </>
  );
}
