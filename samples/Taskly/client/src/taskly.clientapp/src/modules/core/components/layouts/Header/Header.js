import styles from "./Header.module.scss";
import MainNav from "../MainNav/MainNav";
import Logo from "../../Logo/Logo";

const Header = () => {
  return (
    <div className={styles.headerContent}>
      <div className={styles.headerLogoWrapper}>
        <Logo />
      </div>
      <div className={styles.headerMainNavWrapper}>
        <MainNav />
      </div>
    </div>
  );
};

export default Header;
