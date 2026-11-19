/** D1: mọi response Auth/profile có body đều là `{ data, meta }`; bóc `data` tại một chỗ (module auth). */
export interface Envelope<T> {
  data: T;
  meta?: unknown;
}

export const unwrap = <T>(res: Envelope<T>): T => res.data;
