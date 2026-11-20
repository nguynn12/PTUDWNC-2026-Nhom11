"use client";

import Script from "next/script";
import { useEffect, useRef, useState } from "react";

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

// Google chỉ giữ lần initialize() cuối cùng và cảnh báo khi gọi nhiều lần, nên khởi tạo đúng một lần
// cho mỗi lần tải trang; handler luôn trỏ tới component đang hiển thị.
// FR-AUTH-003: nonce ngẫu nhiên cho mỗi lần tải trang, backend đối chiếu với claim "nonce" trong ID token.
let pageNonce: string | null = null;
let initializedClientId: string | null = null;
let activeHandler: ((idToken: string, nonce: string) => void) | null = null;

function getPageNonce(): string {
  pageNonce ??= crypto.randomUUID();
  return pageNonce;
}

export function GoogleSignInButton({ onCredential }: GoogleSignInButtonProps) {
  const slot = useRef<HTMLDivElement>(null);
  const [ready, setReady] = useState(false);
  const clientId = process.env.NEXT_PUBLIC_GOOGLE_CLIENT_ID;

  useEffect(() => {
    activeHandler = onCredential;
    return () => {
      if (activeHandler === onCredential) activeHandler = null;
    };
  }, [onCredential]);

  useEffect(() => {
    const google = window.google;
    if (!ready || !clientId || !slot.current || !google) return;
    if (initializedClientId !== clientId) {
      const nonce = getPageNonce();
      google.accounts.id.initialize({
        client_id: clientId,
        nonce,
        callback: (response) => activeHandler?.(response.credential, nonce),
      });
      initializedClientId = clientId;
    }
    slot.current.replaceChildren();
    google.accounts.id.renderButton(slot.current, {
      theme: "outline",
      size: "large",
      text: "continue_with",
      width: 320,
    });
  }, [ready, clientId]);

  if (!clientId) {
    return (
      <p className="text-center text-xs text-muted-foreground">
        Chưa cấu hình NEXT_PUBLIC_GOOGLE_CLIENT_ID nên chưa dùng được đăng nhập Google.
      </p>
    );
  }

  return (
    <>
      <Script src="https://accounts.google.com/gsi/client" strategy="afterInteractive" onReady={() => setReady(true)} />
      <div ref={slot} className="flex min-h-11 justify-center" />
    </>
  );
}
