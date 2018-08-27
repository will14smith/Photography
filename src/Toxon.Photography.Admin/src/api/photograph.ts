import { fetchWithAuthentication } from "./fetch";

export enum ImageType {
  Full = "Full",
  Thumbnail = "Thumbnail"
}
export interface Image {
  Type: ImageType;
  ObjectKey: string;
}
export interface Photograph {
  Id: string;
  Title: string;

  Images: Image[];
}

export interface PhotographCreate {
  Title: string;

  ImageKey: string;
}

export async function getAll(): Promise<Photograph[]> {
  const response = await fetchWithAuthentication("/photograph", {
    method: "GET"
  });

  if (!response.ok) {
    throw new Error(
      "Failed to get all photographs: " + JSON.stringify(response)
    );
  }

  return await response.json();
}

export async function create(model: PhotographCreate): Promise<Photograph> {
  const response = await fetchWithAuthentication("/photograph", {
    body: JSON.stringify(model),
    headers: { "Content-Type": "application/json" },
    method: "POST"
  });

  if (!response.ok) {
    throw new Error(
      "Failed to get create photography: " + JSON.stringify(response)
    );
  }

  return await response.json();
}
