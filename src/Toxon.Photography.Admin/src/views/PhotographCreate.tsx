import * as React from "react";
import { connect } from "react-redux";

import { Storage } from "aws-amplify";
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
  capture: Date;
}

function generateKey(length = 40) {
  let text = "";
  const possible =
    "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

  for (let i = 0; i < length; i++) {
    text += possible.charAt(Math.floor(Math.random() * possible.length));
  }

  return text;
}

export class PhotographCreate extends React.Component<Props, State> {
  public state: State = { title: "", image: null, capture: new Date() };

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

          <div className="form-group">
            <label htmlFor="capture-date">Capture Date</label>
            <input
              id="capture"
              required
              value={this.state.capture.toISOString().split("T")[0]}
              onChange={this.captureChanged}
              type="date"
              className="form-control"
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
  private captureChanged = (e: React.ChangeEvent<HTMLInputElement>) =>
    this.setState({ capture: new Date(e.target.value) });

  private submit = async (e: React.SyntheticEvent) => {
    e.preventDefault();
    e.stopPropagation();

    const state = this.state;

    if (state.image === null) {
      return;
    }

    const { key }: any = await Storage.put(
      "image/" + generateKey(),
      state.image,
      {
        contentType: state.image.type,
        customPrefix: { public: "" },
        level: "public"
      }
    );

    this.props.createPhotograph(
      {
        Title: state.title,

        ImageKey: key,

        CaptureTime: state.capture
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
    createPhotograph: async (model: PhotographCreateModel) => {
      const response = await create(model);

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
