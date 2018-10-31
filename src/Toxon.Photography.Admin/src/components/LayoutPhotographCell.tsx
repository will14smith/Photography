import * as React from "react";

import { Photograph } from "../api/photograph";
import PhotographThumbnail from "./PhotographThumbnail";

export interface Props {
  photograph: Photograph;
  onClick?: () => void;
}

export default function LayoutPhotographCell({ photograph, onClick }: Props) {
  const width = (photograph.Layout || { Width: 1 }).Width || 1;
  // TODO handle height

  return (
    <div className={`col-md-${4 * width} layout-cell`} onClick={onClick}>
      <div className="card mb-4 box-shadow">
        <PhotographThumbnail photograph={photograph} width="100%" />
      </div>
    </div>
  );
}
