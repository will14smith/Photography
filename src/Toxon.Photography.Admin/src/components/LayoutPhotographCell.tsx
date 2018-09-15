import * as React from "react";

import { Photograph } from "../api/photograph";
import PhotographThumbnail from "./PhotographThumbnail";

export interface Props {
  photograph: Photograph;
  onClick?: () => void;
}

export default function LayoutPhotographCell({ photograph, onClick }: Props) {
  return (
    <div className="col-md-4 layout-cell" onClick={onClick}>
      <div className="card mb-4 box-shadow">
        <PhotographThumbnail photograph={photograph} />
      </div>
    </div>
  );
}
