import { combineReducers } from "redux";

import { DECREMENT_VALUE, INCREMENT_VALUE, ValueAction } from "./actions";
import { ValueState } from "./store";

import { reducer as photographReducer } from "./photograph";

const defaultValueState: ValueState = 1;

export function valueReducer(
  state: ValueState = defaultValueState,
  action: ValueAction
): ValueState {
  switch (action.type) {
    case INCREMENT_VALUE:
      return state + 1;
    case DECREMENT_VALUE:
      return Math.max(1, state - 1);
  }
  return state;
}

const rootReducer = combineReducers({
  photographs: photographReducer,
  value: valueReducer
});

export default rootReducer;
