import * as React from "react";

export interface Props {
  value: number;
  onIncrement?: () => void;
  onDecrement?: () => void;
}

export default function Value({ value, onIncrement, onDecrement }: Props) {
  return (
    <div className="hello">
      <div className="greeting">Hello {value}</div>
      <div>
        <button onClick={onDecrement}>-</button>
        <button onClick={onIncrement}>+</button>
      </div>
    </div>
  );
}
