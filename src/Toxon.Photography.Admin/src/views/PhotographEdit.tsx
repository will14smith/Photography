import * as React from "react";
import { connect } from "react-redux";

import { push } from "connected-react-router";

import {
  edit,
  Photograph,
  PhotographEdit as PhotographEditModel
} from "../api/photograph";
import PhotographThumbnail from "../components/PhotographThumbnail";
import ViewHeader from "../components/ViewHeader";
import * as redux from "../redux/photograph";
import { RootState } from "../redux/store";
import { Dispatch } from "../redux/types";

export interface Props {
  getPhotograph: (id: string) => void;
  editPhotograph: (
    id: string,
    model: PhotographEditModel
  ) => Promise<Photograph>;

  id: string;

  loading: boolean;
  error?: Error;
  photograph?: Photograph;
}

interface State {
  photographId?: string;

  title: string;

  capture: Date;
}

export class PhotographEdit extends React.Component<Props, State> {
  public static getDerivedStateFromProps(
    props: Props,
    state: State
  ): State | null {
    const { photograph } = props;

    if (!photograph || photograph.Id === state.photographId) {
      return null;
    }

    return {
      photographId: photograph.Id,

      title: photograph.Title,

      capture: photograph.CaptureTime
    };
  }

  public state: State = {
    photographId: undefined,

    title: "",

    capture: new Date()
  };

  constructor(props: Props) {
    super(props);
  }

  public componentDidMount() {
    this.props.getPhotograph(this.props.id);
  }

  public render() {
    const { loading, error, photograph } = this.props;

    return (
      <div>
        <ViewHeader title={photograph ? photograph.Title : "Photograph Edit"} />

        <ul className="list-group">
          {loading && (
            <div className="alert alert-light" role="alert">
              Loading...
            </div>
          )}
          {error && (
            <div className="alert alert-danger" role="alert">
              <strong>Error: </strong>
              {error.toString()}
            </div>
          )}
          {photograph && (
            <div>
              <PhotographThumbnail photograph={photograph} width="500px" />

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
                  Save
                </button>
              </form>
            </div>
          )}
        </ul>
      </div>
    );
  }

  private titleChanged = (e: React.ChangeEvent<HTMLInputElement>) =>
    this.setState({ title: e.target.value });
  private captureChanged = (e: React.ChangeEvent<HTMLInputElement>) =>
    this.setState({ capture: new Date(e.target.value) });

  private submit = async (e: React.SyntheticEvent) => {
    e.preventDefault();
    e.stopPropagation();

    const state = this.state;
    this.props.editPhotograph(this.props.id, {
      Title: state.title,

      CaptureTime: state.capture
    });
  };
}

interface RouterProps {
  match: { params: { id: string } };
}

export function mapStateToProps(
  { photographs }: RootState,
  ownProps: RouterProps
) {
  const id = ownProps.match.params.id;

  return {
    id,

    error: photographs.error,
    loading: photographs.loading,
    photograph: photographs.byId[id]
  };
}

export function mapDispatchToProps(dispatch: Dispatch) {
  return {
    getPhotograph: (id: string) => dispatch(redux.getPhotograph(id)),

    editPhotograph: async (id: string, model: PhotographEditModel) => {
      // TODO should this an action creator?
      const response = await edit(id, model);

      dispatch(push(`/photographs/${response.Id}`));

      return response;
    }
  };
}

export default connect(
  mapStateToProps,
  mapDispatchToProps
)(PhotographEdit);
