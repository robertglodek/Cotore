import styles from "./App.module.scss";
import { Component } from "react";

import Header from "./modules/core/components/layouts/Header/Header";
import SideNav from "./modules/core/components/layouts/SideNav/SideNav";
import Footer from "./modules/core/components/layouts/Footer/Footer";
import Page from "./modules/core/components/layouts/Page/Page";
import { BrowserRouter as Router } from "react-router-dom";

class App extends Component {
  render() {
    return (
      <Router>
        <div className={styles.app}>
          <header className={styles.header}>
            <Header />
          </header>
          <main className={styles.main}>
            <aside className={styles.sideNav}>
              <SideNav />
            </aside>
            <section className={styles.page}>
              <Page />
            </section>
          </main>
          <footer className={styles.footer}>
            <Footer />
          </footer>
        </div>
      </Router>
    );
  }
}

export default App;
