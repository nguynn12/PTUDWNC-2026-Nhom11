import type { ReactNode } from "react";

export function ManagementPageHeader({
  title,
  description,
  actions,
}: {
  title: string;
  description: string;
  actions?: ReactNode;
}) {
  return (
    <header className="management-page-header">
      <div>
        <h1>{title}</h1>
        <p>{description}</p>
      </div>
      {actions && <div className="management-page-actions">{actions}</div>}
    </header>
  );
}
