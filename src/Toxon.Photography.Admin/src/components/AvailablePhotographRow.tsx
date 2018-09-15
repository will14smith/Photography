import * as React from "react";

import { Photograph } from "../api/photograph";
import PhotographThumbnail from "./PhotographThumbnail";

export interface Props {
  photograph: Photograph;
  onClick?: () => void;
}

export default function AvailablePhotographRow({ photograph, onClick }: Props) {
  return (
    <li className="list-group-item" onClick={onClick}>
      <div className="row align-items-center">
        <div className="col-sm-auto">
          <PhotographThumbnail photograph={photograph} width="50px" />
        </div>
        <div className="col-sm">
          <h3>{photograph.Title}</h3>
        </div>
      </div>
    </li>
  );
}
