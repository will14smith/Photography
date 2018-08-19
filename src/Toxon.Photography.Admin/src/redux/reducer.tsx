import { DECREMENT_VALUE, INCREMENT_VALUE, ValueAction } from "./actions";
import { ValueState } from "./store";

const defaultState: ValueState = {
  value: 1
};

export default function reducer(
  state: ValueState = defaultState,
  action: ValueAction
): ValueState {
  switch (action.type) {
    case INCREMENT_VALUE:
      return { ...state, value: state.value + 1 };
    case DECREMENT_VALUE:
      return {
        ...state,
        value: Math.max(1, state.value - 1)
      };
  }
  return state;
}
