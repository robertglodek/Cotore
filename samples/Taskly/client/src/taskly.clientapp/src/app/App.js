import styles from "./App.css";
import { Component } from 'react';
import Header from '../shared/layouts/Header';
import SideNav from '../shared/layouts/SideNav';
import Footer from '../shared/layouts/Footer';
import Page from '../shared/layouts/Page';

class App extends Component {
  render() {
    return (
      
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
    );
  }
}

export default App;