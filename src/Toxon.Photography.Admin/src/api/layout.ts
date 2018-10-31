import { fetchWithAuthentication } from "./fetch";

export interface LayoutModel {
  Order: number;

  Width: number | null;
  Height: number | null;
}

export interface Layout {
  [id: string]: LayoutModel;
}

export type LayoutEdit = Layout;

export async function edit(model: LayoutEdit): Promise<void> {
  const response = await fetchWithAuthentication(`/layout`, {
    body: JSON.stringify(model),
    headers: { "Content-Type": "application/json" },
    method: "PUT"
  });

  if (!response.ok) {
    throw new Error("Failed to save layout: " + JSON.stringify(response));
  }
}
