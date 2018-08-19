import { Dispatch } from "redux";

import { getAll, Photograph } from "../api/photograph";
import { Action } from "./types";

// Actions
export const GET_PHOTOGRAPHS_STARTED = "GET_PHOTOGRAPHS_STARTED";
export const GET_PHOTOGRAPHS_SUCCESS = "GET_PHOTOGRAPHS_SUCCEEDED";
export const GET_PHOTOGRAPHS_FAILED = "GET_PHOTOGRAPHS_FAILED";
export type GET_PHOTOGRAPHS_STARTED = typeof GET_PHOTOGRAPHS_STARTED;
export type GET_PHOTOGRAPHS_SUCCEEDED = typeof GET_PHOTOGRAPHS_SUCCESS;
export type GET_PHOTOGRAPHS_FAILED = typeof GET_PHOTOGRAPHS_FAILED;

export interface GetPhotographsStarted {
  type: GET_PHOTOGRAPHS_STARTED;
}
export interface GetPhotographsSucceeded {
  type: GET_PHOTOGRAPHS_SUCCEEDED;
  photographs: Photograph[];
}
export interface GetPhotographsFailed {
  type: GET_PHOTOGRAPHS_FAILED;
  error: Error;
}

export type PhotographAction =
  | GetPhotographsStarted
  | GetPhotographsSucceeded
  | GetPhotographsFailed;

export function getPhotographsStarted(): GetPhotographsStarted {
  return { type: GET_PHOTOGRAPHS_STARTED };
}
export function getPhotographsSucceeded(
  photographs: Photograph[]
): GetPhotographsSucceeded {
  return { type: GET_PHOTOGRAPHS_SUCCESS, photographs };
}
export function getPhotographsFailed(error: Error): GetPhotographsFailed {
  return { type: GET_PHOTOGRAPHS_FAILED, error };
}

export function getPhotographs(): Action {
  return async (dispatch: Dispatch<PhotographAction>) => {
    dispatch(getPhotographsStarted());
    try {
      const photographs = await getAll();
      dispatch(getPhotographsSucceeded(photographs));
    } catch (e) {
      dispatch(getPhotographsFailed(e));
    }
  };
}

// state
export interface State {
  loading: boolean;
  error?: Error;

  byId: { [id: string]: Photograph };
  ids: string[];
}

const initialState: State = {
  loading: false,

  byId: {},
  ids: []
};

// reducer
export function reducer(
  state: State = initialState,
  action: PhotographAction
): State {
  switch (action.type) {
    case GET_PHOTOGRAPHS_STARTED:
      return { ...state, loading: true, error: undefined };

    case GET_PHOTOGRAPHS_SUCCESS: {
      const ids: string[] = [];
      const byId: { [id: string]: Photograph } = {};

      action.photographs.forEach(photograph => {
        ids.push(photograph.Id);
        byId[photograph.Id] = photograph;
      });

      return { ...state, loading: false, error: undefined, byId, ids };
    }

    case GET_PHOTOGRAPHS_FAILED:
      return { ...state, loading: false, error: action.error };

    default:
      return state;
  }
}
