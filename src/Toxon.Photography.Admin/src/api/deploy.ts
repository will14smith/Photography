import { fetchWithAuthentication } from "./fetch";

export async function deploy(): Promise<void> {
  const response = await fetchWithAuthentication(`/generate`, {
    body: "{}",
    headers: { "Content-Type": "application/json" },
    method: "POST"
  });

  if (!response.ok) {
    throw new Error("Failed to trigger deploy: " + JSON.stringify(response));
  }
}
