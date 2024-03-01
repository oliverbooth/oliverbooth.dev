import fs from "fs";
import gulp from "gulp";
import cleanCSS from "gulp-clean-css";
import noop from "gulp-noop";
import rename from "gulp-rename";
import gulpSass from "gulp-sass";
import * as nodeSass from "sass";
const sass = gulpSass(nodeSass);
import sourcemaps from "gulp-sourcemaps";
import ts from "gulp-typescript";
import terser from "gulp-terser";
import webpack from "webpack-stream";

const srcDir = "src";
const destDir = "OliverBooth/wwwroot";
const production = !process.env.DEVELOPMENT;

function compileSCSS() {
    return gulp.src(`${srcDir}/scss/**/*.scss`)
        .pipe(production ? sourcemaps.init() : noop())
        .pipe(sass().on("error", sass.logError))
        .pipe(cleanCSS({compatibility: "ie11"}))
        .pipe(rename({suffix: ".min"}))
        .pipe(production ? sourcemaps.write() : noop())
        .pipe(gulp.dest(`${destDir}/css`));
}

function compileTS() {
    return gulp.src(`${srcDir}/ts/**/*.ts`)
        .pipe(production ? sourcemaps.init() : noop())
        .pipe(ts("tsconfig.json"))
        .pipe(terser())
        .pipe(production ? sourcemaps.write() : noop())
        .pipe(gulp.dest(`tmp/js`));
}

function bundleJS(done) {
    const tasks = fs.readdirSync("tmp/js", {withFileTypes: true})
        .filter(dirent => dirent.isDirectory())
        .map(d => bundleDir(d.name));
    return gulp.parallel(...tasks)(done);

    function bundleDir(directory) {
        return () => gulp.src(`tmp/js/${directory}/${directory}.js`)
            .pipe(production ? sourcemaps.init() : noop())
            .pipe(webpack({mode: "production", output: {filename: `${directory}.min.js`}}))
            .pipe(production ? sourcemaps.write() : noop())
            .pipe(gulp.dest(`${destDir}/js`));
    }
}

function copyJS() {
    return gulp.src(`${srcDir}/ts/**/*.js`)
        .pipe(rename({suffix: ".min"}))
        .pipe(gulp.dest(`${destDir}/js`));
}

function copyCSS() {
    return gulp.src(`${srcDir}/scss/**/*.css`)
        .pipe(rename({suffix: ".min"}))
        .pipe(gulp.dest(`${destDir}/css`));
}

function copyImages() {
    return gulp.src(`${srcDir}/img/**/*.*`)
        .pipe(gulp.dest(`${destDir}/img`));
}

gulp.task("assets", copyImages);
gulp.task("styles", gulp.parallel(compileSCSS, copyCSS));
gulp.task("scripts", gulp.parallel(copyJS, gulp.series(compileTS, bundleJS)));
gulp.task("default", gulp.parallel("styles", "scripts", "assets"));
