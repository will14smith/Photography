import * as React from "react";
import { connect } from "react-redux";

import { Photograph } from "../api/photograph";
import * as redux from "../redux/photograph";
import { RootState } from "../redux/store";
import { Dispatch } from "../redux/types";

import PhotographThumbnail from "../components/PhotographThumbnail";
import ViewHeader from "../components/ViewHeader";

import "./Layout.css";

export interface Props {
  getPhotographs: () => void;

  loading: boolean;
  error?: Error;
  photographs: Photograph[];
}

function AvailablePhotographRow({
  photograph,
  onClick
}: {
  photograph: Photograph;
  onClick?: () => void;
}) {
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

function LayoutPhotographCell({
  photograph,
  onClick
}: {
  photograph: Photograph;
  onClick?: () => void;
}) {
  return (
    <div className="col-md-4 layout-cell">
      <div className="card mb-4 box-shadow">
        <PhotographThumbnail photograph={photograph} />
      </div>
    </div>
  );
}

class Layout extends React.Component<Props> {
  constructor(props: Props) {
    super(props);
  }

  public componentDidMount() {
    this.props.getPhotographs();
  }

  public render() {
    if (this.props.loading) {
      return <h1>Loading...</h1>;
    }
    if (this.props.error) {
      return (
        <h1>
          <strong>Error: </strong>
          {this.props.error.toString()}
        </h1>
      );
    }

    const layoutPhotos = this.props.photographs;
    const availablePhotos = this.props.photographs;

    return (
      <div className="container-fluid">
        <div className="row layout-header">
          <div className="col">
            <ViewHeader title="Layout" />
          </div>
        </div>
        <div className="row">
          <div className="col-sm-9">
            <div className="container">
              <div className="row pr-4 align-items-center">
                {layoutPhotos.map(photograph => (
                  <LayoutPhotographCell
                    key={photograph.Id}
                    photograph={photograph}
                  />
                ))}
                {layoutPhotos.length !== 0 || (
                  <div className="col-md-12">
                    <h3 className="text-muted">No images, add one!</h3>
                  </div>
                )}
              </div>
            </div>
          </div>
          <div className="col-sm-3 layout-available">
            <h2 className="column-header">Available</h2>
            <ul className="list-group">
              {availablePhotos.length !== 0 ? (
                availablePhotos.map(photograph => (
                  <AvailablePhotographRow
                    key={photograph.Id}
                    photograph={photograph}
                  />
                ))
              ) : (
                <li className="list-group-item list-group-item-light">
                  No photographs available
                </li>
              )}
            </ul>{" "}
          </div>
        </div>
      </div>
    );
  }
}

export function mapStateToProps({ photographs }: RootState) {
  const all = photographs.ids.map(id => photographs.byId[id]);

  return {
    error: photographs.error,
    loading: photographs.loading,
    photographs: all
  };
}

export function mapDispatchToProps(dispatch: Dispatch) {
  return {
    getPhotographs: () => dispatch(redux.getPhotographs())
  };
}

export default connect(
  mapStateToProps,
  mapDispatchToProps
)(Layout);
