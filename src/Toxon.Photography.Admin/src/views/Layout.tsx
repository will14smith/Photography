import * as React from "react";
import * as Icon from "react-feather";
import { connect } from "react-redux";

import { Photograph } from "../api/photograph";
import * as layoutRedux from "../redux/layout";
import * as photographRedux from "../redux/photograph";
import { RootState } from "../redux/store";
import { Dispatch } from "../redux/types";

import AvailablePhotographRow from "../components/AvailablePhotographRow";
import LayoutPhotographCell from "../components/LayoutPhotographCell";
import ViewHeader from "../components/ViewHeader";

import "./Layout.css";

export interface Props {
  getPhotographs: () => void;

  resetLayout: () => void;
  saveLayout: (ids: string[]) => void;
  setLayout: (ids: string[]) => void;

  loading: boolean;
  error?: Error;

  photographs: Photograph[];
  layout: string[];
}

class Layout extends React.Component<Props> {
  constructor(props: Props) {
    super(props);
  }

  public componentDidMount() {
    this.props.getPhotographs();
  }

  public componentWillUnmount() {
    this.props.resetLayout();
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

    const layoutPhotos = this.props.photographs
      .filter(x => x.LayoutPosition !== undefined)
      .sort((a, b) => (a.LayoutPosition || 0) - (b.LayoutPosition || 0));
    const availablePhotos = this.props.photographs.filter(
      x => x.LayoutPosition === undefined
    );

    return (
      <div className="container-fluid">
        <div className="row layout-header">
          <div className="col">
            <ViewHeader title="Layout">
              <a
                onClick={this.save}
                className="btn btn-sm btn-outline-secondary"
              >
                <Icon.Save />
              </a>
            </ViewHeader>
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
                    onClick={this.movePhotograph.bind(this, photograph.Id)}
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
                    onClick={this.addPhotograph.bind(this, photograph.Id)}
                  />
                ))
              ) : (
                <li className="list-group-item list-group-item-light">
                  No photographs available
                </li>
              )}
            </ul>
          </div>
        </div>
      </div>
    );
  }

  private save = () => {
    this.props.saveLayout(this.props.layout);
  };

  private addPhotograph = (photographId: string) => {
    const { layout, setLayout } = this.props;

    setLayout([...layout, photographId]);
  };
  private movePhotograph = (photographId: string) => {
    const { layout, setLayout } = this.props;

    const index = layout.indexOf(photographId);
    if (index + 1 === layout.length) {
      setLayout(layout.slice(0, layout.length - 1));
    } else {
      const newLayout = [...layout];
      newLayout[index] = layout[index + 1];
      newLayout[index + 1] = photographId;
      setLayout(newLayout);
    }
  };
}

function findLayoutPosition(layout: string[], id: string): number | undefined {
  const index = layout.indexOf(id);

  if (index === -1) {
    return undefined;
  }
  return index;
}

export function mapStateToProps({ photographs, layout }: RootState) {
  const currentLayout = layout.currentLayout;
  const all = photographs.ids.map(id => ({
    ...photographs.byId[id],
    LayoutPosition: findLayoutPosition(currentLayout, id)
  }));

  return {
    error: photographs.error,
    loading: photographs.loading,

    layout: currentLayout,
    photographs: all
  };
}

export function mapDispatchToProps(dispatch: Dispatch) {
  return {
    getPhotographs: () => dispatch(photographRedux.getPhotographs()),

    resetLayout: () => dispatch(layoutRedux.resetLayout()),
    saveLayout: (ids: string[]) => dispatch(layoutRedux.saveLayout(ids)),
    setLayout: (ids: string[]) => dispatch(layoutRedux.setLayout(ids))
  };
}

export default connect(
  mapStateToProps,
  mapDispatchToProps
)(Layout);
