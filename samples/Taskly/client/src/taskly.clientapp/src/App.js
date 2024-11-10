import styles from "./App.module.css";
import { Component } from 'react';
import Header from '../shared/layouts/Header';
import SideNav from '../shared/layouts/SideNav';
import Footer from '../shared/layouts/Footer';
import Page from '../shared/layouts/Page';
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