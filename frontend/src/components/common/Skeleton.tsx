import React from "react";

export function RecipeCardSkeleton() {
  return (
    <div className="card" style={{ height: "100%", display: "flex", flexDirection: "column" }}>
      <div className="skeleton skeleton-img" style={{ height: "200px", borderRadius: 0 }} />
      <div className="card-body" style={{ display: "flex", flexDirection: "column", gap: "12px", flex: 1 }}>
        <div style={{ display: "flex", gap: "8px" }}>
          <div className="skeleton" style={{ height: "20px", width: "70px", borderRadius: "9999px" }} />
          <div className="skeleton" style={{ height: "20px", width: "50px", borderRadius: "9999px" }} />
        </div>
        <div className="skeleton skeleton-title" style={{ width: "90%", height: "22px" }} />
        <div className="skeleton skeleton-text" style={{ width: "100%" }} />
        <div className="skeleton skeleton-text" style={{ width: "60%" }} />
        <div style={{ marginTop: "auto", paddingTop: "12px", borderTop: "1px solid var(--border-subtle)", display: "flex", alignItems: "center", gap: "10px" }}>
          <div className="skeleton skeleton-avatar" style={{ width: "28px", height: "28px" }} />
          <div className="skeleton skeleton-text" style={{ width: "90px", height: "12px", margin: 0 }} />
        </div>
      </div>
    </div>
  );
}

export function CategoryCardSkeleton() {
  return (
    <div className="card" style={{ padding: "16px", textAlign: "center", display: "flex", flexDirection: "column", alignItems: "center", gap: "10px" }}>
      <div className="skeleton" style={{ width: "56px", height: "56px", borderRadius: "9999px" }} />
      <div className="skeleton" style={{ width: "70%", height: "16px", borderRadius: "4px" }} />
      <div className="skeleton" style={{ width: "45%", height: "12px", borderRadius: "4px" }} />
    </div>
  );
}

export function RecipeDetailSkeleton() {
  return (
    <div className="container section">
      <div style={{ maxWidth: "880px", margin: "0 auto", display: "flex", flexDirection: "column", gap: "24px" }}>
        <div style={{ display: "flex", gap: "10px" }}>
          <div className="skeleton" style={{ width: "90px", height: "24px", borderRadius: "9999px" }} />
          <div className="skeleton" style={{ width: "60px", height: "24px", borderRadius: "9999px" }} />
        </div>
        <div className="skeleton" style={{ width: "85%", height: "42px", borderRadius: "8px" }} />
        <div className="skeleton" style={{ width: "100%", height: "18px" }} />
        <div className="skeleton" style={{ width: "75%", height: "18px" }} />
        
        {/* Hero image skeleton */}
        <div className="skeleton" style={{ width: "100%", height: "420px", borderRadius: "16px" }} />
        
        {/* Quick stats skeleton */}
        <div style={{ display: "grid", gridTemplateColumns: "repeat(4, 1fr)", gap: "16px" }}>
          <div className="skeleton" style={{ height: "70px", borderRadius: "10px" }} />
          <div className="skeleton" style={{ height: "70px", borderRadius: "10px" }} />
          <div className="skeleton" style={{ height: "70px", borderRadius: "10px" }} />
          <div className="skeleton" style={{ height: "70px", borderRadius: "10px" }} />
        </div>

        {/* Content sections skeleton */}
        <div className="skeleton" style={{ width: "100%", height: "200px", borderRadius: "12px" }} />
        <div className="skeleton" style={{ width: "100%", height: "280px", borderRadius: "12px" }} />
      </div>
    </div>
  );
}
