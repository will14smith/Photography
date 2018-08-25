import { Auth } from "aws-amplify";

import { ENDPOINT } from "./config";

export interface Photograph {
  Id: string;
  Title: string;
}

export interface PhotographCreate {
  Title: string;
}

export async function getAll(): Promise<Photograph[]> {
  const session = await Auth.currentSession();
  const token = session.idToken.jwtToken;

  const response = await fetch(ENDPOINT + "/photograph", {
    headers: { Authorization: "Bearer " + token },
    method: "GET"
  });

  if (!response.ok) {
    throw new Error("Failed to get all photographs: " + response);
  }

  return await response.json();
}

export async function create(model: PhotographCreate): Promise<Photograph> {
  const response = await fetch(ENDPOINT + "/photograph", {
    body: JSON.stringify(model),
    headers: { "Content-Type": "application/json" },
    method: "POST"
  });

  if (!response.ok) {
    throw new Error("Failed to get create photography: " + response);
  }

  return await response.json();
}
