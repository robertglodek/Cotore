import styles from "../styles/layouts/Page.module.css";
import { Route, Routes } from "react-router-dom";

const Page = () => {
  return (
    <div className={styles.pageContent}>
      <Routes>
        <Route/>
      </Routes>
    </div>
  );
};

export default Page;