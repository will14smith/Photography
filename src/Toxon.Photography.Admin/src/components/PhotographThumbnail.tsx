import * as React from "react";

import { ImageType, Photograph } from "../api/photograph";
import S3Image from "./S3Image";

export interface Props {
  photograph: Photograph;
  width: string;
}

export default function PhotographThumbnail({ photograph, width }: Props) {
  const thumbnail = photograph.Images.find(x => x.Type === ImageType.Thumbnail);
  if (!thumbnail) {
    return <img src={`https://via.placeholder.com/${width}`} />;
  }

  return <S3Image imageKey={thumbnail.ObjectKey} style={{ width }} />;
}
