import { ThunkAction, ThunkDispatch } from "redux-thunk";

import { RootAction } from "./actions";
import { RootState } from "./store";

export type Action = ThunkAction<void, RootState, null, RootAction>;
export type Dispatch = ThunkDispatch<RootState, null, RootAction>;
