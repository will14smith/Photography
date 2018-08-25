import * as React from "react";
import { connect } from "react-redux";

import { push } from "connected-react-router";

import {
  create,
  Photograph,
  PhotographCreate as PhotographCreateModel
} from "../api/photograph";
import ViewHeader from "../components/ViewHeader";
import { Dispatch } from "../redux/types";

export interface Props {
  createPhotograph: (
    model: PhotographCreateModel,
    onProgress?: ((evt: ProgressEvent) => any)
  ) => Promise<Photograph>;
}

interface State {
  title: string;
  image: File | null;
}

function readBlobAsBase64(blob: Blob): Promise<string> {
  const reader = new FileReader();

  return new Promise((resolve, reject) => {
    reader.onload = () => {
      const dataUri = reader.result as string;

      const startOfContent = dataUri.indexOf("base64,") + "base64,".length;

      resolve(dataUri.slice(startOfContent));
    };
    reader.onabort = reject;
    reader.onerror = reject;

    reader.readAsDataURL(blob);
  });
}

export class PhotographCreate extends React.Component<Props, State> {
  public state: State = { title: "", image: null };

  constructor(props: Props) {
    super(props);
  }

  public render() {
    return (
      <div>
        <ViewHeader title="Create Photograph" />

        <form onSubmit={this.submit}>
          <div className="form-group">
            <label htmlFor="title">Title</label>
            <input
              id="title"
              placeholder="Enter photograph title"
              required
              value={this.state.title}
              onChange={this.titleChanged}
              type="text"
              className="form-control"
            />
          </div>

          <div className="form-group">
            <label htmlFor="image">Image</label>
            <input
              id="image"
              required
              onChange={this.imageChanged}
              type="file"
              className="form-control-file"
            />
          </div>

          <button type="submit" className="btn btn-primary">
            Create
          </button>
        </form>
      </div>
    );
  }

  private titleChanged = (e: React.ChangeEvent<HTMLInputElement>) =>
    this.setState({ title: e.target.value });
  private imageChanged = (e: React.ChangeEvent<HTMLInputElement>) =>
    this.setState({ image: e.target.files ? e.target.files[0] : null });

  private submit = async (e: React.SyntheticEvent) => {
    e.preventDefault();
    e.stopPropagation();

    const state = this.state;

    if (state.image === null) {
      return;
    }

    const imageBase64 = await readBlobAsBase64(state.image);

    this.props.createPhotograph(
      {
        Title: state.title,

        ImageBase64: imageBase64,
        ImageContentType: state.image.type
      },
      this.updateProgress
    );
  };
  private updateProgress = (evt: ProgressEvent) => {
    // tslint:disable-next-line
    console.log(evt);
  };
}

export function mapDispatchToProps(dispatch: Dispatch) {
  return {
    createPhotograph: async (
      model: PhotographCreateModel,
      onProgress?: ((evt: ProgressEvent) => any)
    ) => {
      const response = await create(model, onProgress);

      // TODO redirect to photograph details
      dispatch(push("/photographs"));

      return response;
    }
  };
}

export default connect(
  null,
  mapDispatchToProps
)(PhotographCreate);
