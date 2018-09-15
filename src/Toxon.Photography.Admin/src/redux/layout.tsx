import { Dispatch } from "redux";

import * as layoutApi from "../api/layout";
import {
  getPhotographs,
  GetPhotographsSucceeded,
  PhotographAction
} from "./photograph";
import { RootState } from "./store";
import { Action } from "./types";

// Actions

export const RESET_LAYOUT = "RESET_LAYOUT";
export const SET_LAYOUT = "SET_LAYOUT";
export type RESET_LAYOUT = typeof RESET_LAYOUT;
export type SET_LAYOUT = typeof SET_LAYOUT;

export interface ResetLayout {
  type: RESET_LAYOUT;
}
export interface SetLayout {
  type: SET_LAYOUT;
  ids: string[];
}

export function resetLayout(): ResetLayout {
  return { type: RESET_LAYOUT };
}
export function saveLayout(ids: string[]): Action {
  return async (
    dispatch: Dispatch<LayoutAction | PhotographAction>,
    getState: () => RootState
  ) => {
    const model = {};
    ids.forEach((id, idx) => (model[id] = idx));
    await layoutApi.edit(model);

    return getPhotographs()(dispatch, getState, null);
  };
}
export function setLayout(ids: string[]): SetLayout {
  return { type: SET_LAYOUT, ids };
}

export type LayoutAction = ResetLayout | SetLayout;

// State

export interface State {
  initialLayout: string[];

  currentLayout: string[];
}

const initialState: State = {
  initialLayout: [],

  currentLayout: []
};

// Reducer
export function reducer(
  state: State = initialState,
  action: LayoutAction | GetPhotographsSucceeded
): State {
  switch (action.type) {
    case "GET_PHOTOGRAPHS_SUCCEEDED":
      const layout = action.photographs
        .filter(x => x.LayoutPosition !== undefined)
        .sort((a, b) => (a.LayoutPosition || 0) - (b.LayoutPosition || 0))
        .map(x => x.Id);

      return { initialLayout: layout, currentLayout: layout };

    case SET_LAYOUT:
      return { ...state, currentLayout: action.ids };

    case RESET_LAYOUT:
      return { ...state, currentLayout: state.initialLayout };
  }
  return state;
}
