import axios from "axios";
import type { HealthReport } from "@/types/api";

const isServer = typeof window === "undefined";
const backendOrigin = isServer
  ? process.env.BACKEND_API_URL || "http://localhost:5000"
  : "";

/**
 * FR-OBS-001: Kiểm tra tổng hợp tình trạng sức khỏe của PostgreSQL, Redis Cache và MinIO
 */
export async function getHealth(): Promise<HealthReport> {
  const url = `${backendOrigin}/health`;
  const response = await axios.get<HealthReport>(url, {
    timeout: 10000,
  });
  return response.data;
}

/**
 * FR-OBS-001: Liveness probe kiểm tra ứng dụng .NET còn sống
 */
export async function getHealthLive(): Promise<{ status: string }> {
  const url = `${backendOrigin}/health/live`;
  const response = await axios.get<{ status: string }>(url, {
    timeout: 5000,
  });
  return response.data;
}
