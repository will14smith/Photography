import * as React from "react";

export interface Props {
  title: string;
  children?: React.ReactNode;
}

export default function ViewHeader({ title, children }: Props) {
  return (
    <div className="d-flex justify-content-between flex-wrap flex-md-nowrap align-items-center pt-3 pb-2 mb-3 border-bottom">
      <h1 className="h2">Hello World!</h1>
      {children && (
        <div className="btn-toolbar mb-2 mb-md-0">
          <div className="btn-group mr-2">{children}</div>
        </div>
      )}
    </div>
  );
}
