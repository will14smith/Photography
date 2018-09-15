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
  LayoutPosition?: number;

  Images: Image[];

  CaptureTime: Date;
  UploadTime: Date;
}

export interface PhotographCreate {
  Title: string;

  ImageKey: string;

  CaptureTime: Date;
}
export interface PhotographEdit {
  Title: string;

  CaptureTime: Date;
}

function toAppModel(apiModel: any): Photograph {
  return {
    ...apiModel,
    CaptureTime: new Date(apiModel.CaptureTime),
    UploadTime: new Date(apiModel.UploadTime)
  };
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

  const apiModels = await response.json();

  return apiModels.map(toAppModel);
}

export async function get(id: string): Promise<Photograph> {
  const response = await fetchWithAuthentication(`/photograph/${id}`, {
    method: "GET"
  });

  if (!response.ok) {
    throw new Error("Failed to get photograph: " + JSON.stringify(response));
  }

  const apiModel = await response.json();
  return toAppModel(apiModel);
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

  const apiModel = await response.json();
  return toAppModel(apiModel);
}

export async function edit(
  id: string,
  model: PhotographEdit
): Promise<Photograph> {
  const response = await fetchWithAuthentication(`/photograph/${id}`, {
    body: JSON.stringify(model),
    headers: { "Content-Type": "application/json" },
    method: "PUT"
  });

  if (!response.ok) {
    throw new Error(
      "Failed to get edit photography: " + JSON.stringify(response)
    );
  }

  const apiModel = await response.json();
  return toAppModel(apiModel);
}
