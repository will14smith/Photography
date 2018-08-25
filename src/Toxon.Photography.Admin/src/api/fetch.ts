import { Auth } from "aws-amplify";

import { ENDPOINT } from "./config";

async function requestWithAuthentication(
  init: RequestInit
): Promise<RequestInit> {
  const session = await Auth.currentSession();
  const token = session.idToken.jwtToken;

  return {
    ...init,
    headers: { ...init.headers, Authorization: "Bearer " + token }
  };
}

export async function fetchWithAuthentication(
  url: string,
  init: RequestInit = {}
): Promise<Response> {
  const authenticatedInit = await requestWithAuthentication(init);

  return fetch(ENDPOINT + url, authenticatedInit);
}

export interface XHRResponse {
  body: string;

  ok: boolean;
  status: number;
}

export async function fetchWithProgress(
  url: string,
  request: RequestInit,
  onProgress?: ((evt: ProgressEvent) => any)
): Promise<XHRResponse> {
  const authenticatedRequest = await requestWithAuthentication(request);

  const xhr = new XMLHttpRequest();

  return new Promise<XHRResponse>((resolve, reject) => {
    xhr.onerror = reject;
    xhr.onabort = reject;
    xhr.onprogress = onProgress || null;
    xhr.onreadystatechange = () => {
      if (xhr.readyState !== XMLHttpRequest.DONE) {
        return;
      }

      resolve({
        body: xhr.responseText,

        ok: xhr.status >= 200 && xhr.status <= 299,
        status: xhr.status
      });
    };

    const headers = authenticatedRequest.headers || {};
    xhr.open(authenticatedRequest.method || "GET", ENDPOINT + url);
    Object.keys(headers).forEach(headerKey =>
      xhr.setRequestHeader(headerKey, headers[headerKey])
    );
    xhr.send(authenticatedRequest.body || undefined);
  });
}
