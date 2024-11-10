import styles from '../styles/layouts/Header.module.css'
import MainNav from './MainNav';

const Header = () => {
  return (
    <div className={styles.headerContent}>
          <nav className={styles.mainNav}>
            <MainNav/>
          </nav>
    </div>
  );
};

export default Header;
