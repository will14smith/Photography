export const INCREMENT_VALUE = "INCREMENT_VALUE";
export type INCREMENT_VALUE = typeof INCREMENT_VALUE;

export const DECREMENT_VALUE = "DECREMENT_VALUE";
export type DECREMENT_VALUE = typeof DECREMENT_VALUE;

export interface IncrementValue {
  type: INCREMENT_VALUE;
}

export interface DecrementValue {
  type: DECREMENT_VALUE;
}

export type ValueAction = IncrementValue | DecrementValue;

export function incrementValue(): IncrementValue {
  return {
    type: INCREMENT_VALUE
  };
}

export function decrementValue(): DecrementValue {
  return {
    type: DECREMENT_VALUE
  };
}
