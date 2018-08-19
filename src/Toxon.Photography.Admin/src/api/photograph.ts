import { ENDPOINT } from "./config";

export interface Photograph {
  Id: string;
  Title: string;
}

export async function getAll(): Promise<Photograph[]> {
  const response = await fetch(ENDPOINT + "/photograph", { method: "GET" });

  if (!response.ok) {
    throw new Error("Failed to get all photographs: " + response);
  }

  return await response.json();
}
