import { Lora } from "next/font/google";

// Lora cho tiêu đề (hỗ trợ đủ dấu tiếng Việt; Playfair Display làm vỡ dấu ở chữ như "Luyến").
// Chữ giao diện giữ Plus Jakarta Sans của toàn site để đồng nhất với header.
const lora = Lora({ subsets: ["latin", "vietnamese"], variable: "--font-lora", display: "swap" });

/**
 * Class bọc khu vực tài khoản: token màu theo phạm vi `.cb-account` (globals.css) và biến font Lora.
 * Hộp thoại (portal ra ngoài khu vực) cũng cần class này để nhận cùng token.
 */
export const PROFILE_THEME_CLASS = `cb-account ${lora.variable}`;
