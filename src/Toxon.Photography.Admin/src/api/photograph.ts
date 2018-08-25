import { fetchWithAuthentication, fetchWithProgress } from "./fetch";

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

  Image: Image[];
}

export interface PhotographCreate {
  Title: string;

  ImageBase64: string;
  ImageContentType: string;
}

export async function getAll(): Promise<Photograph[]> {
  const response = await fetchWithAuthentication("/photograph", {
    method: "GET"
  });

  if (!response.ok) {
    throw new Error("Failed to get all photographs: " + response);
  }

  return await response.json();
}

export async function create(
  model: PhotographCreate,
  onProgress?: ((evt: ProgressEvent) => any)
): Promise<Photograph> {
  const response = await fetchWithProgress(
    "/photograph",
    {
      body: JSON.stringify(model),
      headers: { "Content-Type": "application/json" },
      method: "POST"
    },
    onProgress
  );

  if (!response.ok) {
    throw new Error("Failed to get create photography: " + response);
  }

  return JSON.parse(response.body);
}
