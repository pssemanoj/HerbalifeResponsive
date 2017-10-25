/// <binding />
var path = require("path");
var vertical = "Ordering/";
var files = [];
var newFiles = [];
var actualFile;
var activeCSSBundle = false;
var scriptsPath = vertical + 'Scripts/bundle/hashfiles/';
var cssPath = vertical + 'CSS/bundle/hashfiles/'


module.exports = function (grunt) {
    function getFiles(txtPath, txtName, prefix, dest) {
        result = grunt.file.read(path.resolve(txtPath) + path.sep + txtName);
        res2 = result.split("\n");
        res3 = [];
        for (key in res2) {
            res3.push(prefix + res2[key]);
        }
        ret = {};
        ret[dest] = res3;
        return ret;
    };

    grunt.initConfig({
        pkg: grunt.file.readJSON('package.json'),

        //Make CSS 'bundle' 
        cssmin: {
            sitecss: {
                options: {
                    banner: '/*! <%= pkg.name %> - v<%= pkg.version %> - ' +
                        '<%= grunt.template.today("yyyy-mm-dd") %> */',
                },
                files: getFiles(vertical + "CSS", "mainCSS.txt", "", vertical + "CSS/bundle/site.css")
            },
            legacysitecss1: {
                options: {
                    banner: '/*! <%= pkg.name %> - v<%= pkg.version %> - ' +
                        '<%= grunt.template.today("yyyy-mm-dd") %> */',
                    advanced: false
                },
                files: getFiles("SharedUI", "CommonLegacyCSS1.txt", "SharedUI/CSS/", vertical + "CSS/bundle/legacysite1.css")
            },
            legacysitecss2: {
                options: {
                    banner: '/*! <%= pkg.name %> - v<%= pkg.version %> - ' +
                        '<%= grunt.template.today("yyyy-mm-dd") %> */',
                    advanced: false
                },
                files: getFiles("SharedUI", "CommonLegacyCSS2.txt", "SharedUI/CSS/", vertical + "CSS/bundle/legacysite2.css")
            },
            legacysitecss3: {
                options: {
                    banner: '/*! <%= pkg.name %> - v<%= pkg.version %> - ' +
                        '<%= grunt.template.today("yyyy-mm-dd") %> */',
                    advanced: false
                },
                files: getFiles("SharedUI", "CommonLegacyCSS3.txt", "SharedUI/CSS/", vertical + "CSS/bundle/legacysite3.css")
            },
            IEcss: {
                options: {
                    banner: '/*! <%= pkg.name %> - v<%= pkg.version %> - ' +
                        '<%= grunt.template.today("yyyy-mm-dd") %> */',
                    advanced: false
                },
                files: [
                    { src: [vertical + 'CSS/main-ordering_1.css'], dest: vertical + 'CSS/bundle/site_1.css' },
                    { src: [vertical + 'CSS/main-ordering_2.css'], dest: vertical + 'CSS/bundle/site_2.css' },
                    { src: [vertical + 'CSS/main-ordering_3.css'], dest: vertical + 'CSS/bundle/site_3.css' }
                ]
            },
            responsivecss: {
                options: {
                    banner: '/*! <%= pkg.name %> - v<%= pkg.version %> - ' +
                        '<%= grunt.template.today("yyyy-mm-dd") %> */',
                    advanced: false
                },
                files: [
                    { src: [vertical + 'CSS/ordering-responsive.css'], dest: vertical + 'CSS/bundle/responsive-site.css' }
                ]
            },

        },

        //make JS bundle
        uglify: {
            options: {
                //mangle: false,
                sourceMap: true,
                sourceMapIncludeSources: true
            },
            Common: {
                files: getFiles("SharedUI", "commonJS.txt", "SharedUI/Scripts/", vertical + "Scripts/bundle/common.js")
            },
            Vertical: {
                files: getFiles(vertical + "Scripts", "vertical.txt", vertical + "Scripts/bundle/vertical.js")
            },
            LegacyCommon: {
                files: getFiles(vertical + "Scripts", "commonLegacyJS.txt", '', vertical + "Scripts/bundle/legacycommon.js")
            },
            LegacyVertical: {
                files: getFiles(vertical + "Scripts", "legacyVertical.txt", "", vertical + "Scripts/bundle/legacyvertical.js")
            }
        },

        clean: {
            all: [vertical + 'Scripts/bundle/common.js', vertical + 'Scripts/bundle/vertical.js', vertical + 'CSS/bundle/site*.css', vertical + 'CSS/bundle/responsive-site*.css'],
            legacyall: [vertical + 'Scripts/bundle/legacycommon.js', vertical + 'Scripts/bundle/legacyvertical.js', vertical + 'CSS/bundle/legacysite*.css'],
            commonJS: [vertical + 'Scripts/bundle/common.js', vertical + 'Scripts/bundle/common.js.map'],
            verticalJS: [vertical + 'Scripts/bundle/vertical.js', vertical + 'Scripts/bundle/vertical.js.map'],
            css: [vertical + 'CSS/bundle/site*.css', vertical + 'CSS/bundle/responsive-site*.css'],
            hashCommonJS: [scriptsPath + "common*.js"],
            hashVerticalJS: [scriptsPath + "vertical*.js"],
            hashCSS: [cssPath + "*.css"]
        },

        shell: {
            // task options
            options: {
                stdout: true,
                stdin: false,
                execOptions: {
                    cwd: "C:/Program Files (x86)/Microsoft Visual Studio 14.0/Common7/IDE"
                }
            },

            checkoutbundles: {
                command: "tf.exe checkout " + path.resolve(vertical + "Scripts/bundle") + path.sep + "common.js " + path.resolve(vertical + "Scripts/bundle") + path.sep + "vertical.js "
                    + path.resolve(vertical + "Scripts/bundle") + path.sep + "common.js.map " + path.resolve(vertical + "Scripts/bundle") + path.sep + "vertical.js.map "
                    + path.resolve(vertical + "CSS/bundle") + path.sep + "site*.css "
                    + path.resolve(vertical + "CSS/bundle") + path.sep + "responsive-site*.css"
            },

            checkoutlegacybundles: {
                command: "tf.exe checkout " + path.resolve(vertical + "Scripts/bundle") + path.sep + "legacycommon.js " + path.resolve(vertical + "Scripts/bundle") + path.sep + "legacyvertical.js "
                    + path.resolve("CSS/bundle") + path.sep + "legacysite*.css"
            },

            // target to checkout all css
            checkoutCSS: {
                command: "tf.exe checkout " + path.resolve(vertical + "CSS") + path.sep + "main-ordering*.css "
                    + path.resolve(vertical + "CSS") + path.sep + "main-ordering.css.map "
                    + path.resolve(vertical + "CSS") + path.sep + "ordering-responsive*.css "
                    + path.resolve(vertical + "CSS") + path.sep + "ordering-responsive.css.map "
            },

            undoCSS: {
                command: "tf.exe undo " + path.resolve(vertical + "CSS") + path.sep + "main-root*.css " + path.resolve(vertical + "CSS") + path.sep + "main-root.css.map"
            },

            checkoutCommonJS: {
                command: "tf.exe checkout " + path.resolve(vertical + "Scripts/bundle") + path.sep + "common.js " + path.resolve(vertical + "Scripts/bundle") + path.sep + "common.js.map " + path.resolve("Views/Shared") + path.sep + "_commonjs.cshtml " + path.resolve("Controls/Template") + path.sep + "commonjs.ascx"
            },

            checkoutVerticalJS: {
                command: "tf.exe checkout " + path.resolve(vertical + "Scripts/bundle") + path.sep + "vertical.js " + path.resolve(vertical + "Scripts/bundle") + path.sep + "vertical.js.map " + path.resolve("Views/Shared") + path.sep + "_verticaljs.cshtml " + path.resolve("Controls/Template") + path.sep + "verticaljs.ascx"
            },

            checkoutCSSbundles: {
                command: "tf.exe checkout " + path.resolve(vertical + "CSS/bundle") + path.sep + "site*.css " + path.resolve("Views/Shared") + path.sep + "_verticalCSS.cshtml " + path.resolve("Controls/Template") + path.sep + "verticalCSS.ascx"
            },

            unmod: {
                command: "tf.exe status " + path.resolve() + " /recursive"
            },
            // === JS
            removeOldFiles: {
                command: function (file) {
                    var com = "tf.exe undo ";
                    switch (file) {
                        case 'common':
                            com += path.resolve(scriptsPath) + path.sep + "common*.js";
                            break;
                        case 'vertical':
                            com += path.resolve(scriptsPath) + path.sep + "vertical*.js";
                            break;
                        case 'css':
                            com += path.resolve(cssPath) + path.sep + "*.css";
                    }
                    com += " /recursive";
                    return com;
                }
            },
            checkoutMapJSON: {
                command: "tf.exe checkout " + path.resolve("assetMaps.json")
            },
            addNewFiles: {
                command: "tf.exe add " + path.resolve("<%= grunt.task.current.args[0] %>") + " /recursive /noignore /noprompt"
            },
            reconcileHash: {
                command: "tf.exe get <%= grunt.task.current.args[0] %> /force"
            }
            
        },

        csssplit: {
            IEcss: {
                src: [vertical + 'CSS/main-ordering.css'],
                dest: vertical + 'CSS/main-ordering.css',
                options: {
                    maxSelectors: 4095,
                    maxPages: 3,
                    suffix: '_'
                }
            },
        },

        // Grunt-sass 
        sass: {
            dist: {
                files: [
                    {
                        expand: true,     // Enable dynamic expansion.
                        cwd: vertical + 'sass/',      // Src matches are relative to this path.
                        src: ['*.scss'], // Actual pattern(s) to match.
                        dest: vertical + 'CSS/',   // Destination path prefix.
                        ext: '.css',   // Dest filepaths will have this extension.
                    },
                ],
            },
            options: {
                sourceMap: true,
                outputStyle: 'expanded',
                includePaths: ["node_modules"]
            }
        },

        sync: {
            sharedJS: {
                files: [{
                    cwd: '../../../../../SharedUI/Dev/R2/Scripts',
                    src: ['**'],
                    dest: 'SharedUI/Scripts'
                }],
                compareUsing: "md5" // compares via md5 hash of file contents, instead of file modification time. Default: "mtime" 
            },
            sharedSass: {
                files: [{
                    cwd: '../../../../../SharedUI/Dev/R2/sass',
                    src: ['**'],
                    dest: 'SharedUI/sass'
                }],
                compareUsing: "md5" // compares via md5 hash of file contents, instead of file modification time. Default: "mtime" 
            },
        },

        //check if files are different
        diff: {
            doCSS: {
                src: ['SharedUI/sass/**/*.{scss,sass}', vertical + 'sass/**/*.{scss,sass}'],
                tasks: ['doCSS', 'doCSSBundle']
            },
            JS: {
                files: [{
                    expand: true,
                    cwd: vertical + "Scripts",
                    src: ['*.js'],
                }],
                tasks: ['doVerticalJSBundle']
            },
            sharedJS: {
                files: [{
                    expand: true,
                    cwd: vertical + "SharedUI/Scripts",
                    src: ['**/*.js'],
                }],
                tasks: ['doCommonJSBundle']
            }
        },

        //All task that are under watch
        watch: {
            sass: {
                // Watches all Sass or Scss files within the scss folder and one level down. 
                // If you want to watch all scss files instead, use the "**/*" globbing pattern
                files: ['**/*.{scss,sass}'],
                tasks: ['diff:doCSS'],
                options: {
                    event: ['added', 'changed'],
                },
            },
            JS: {
                files: [vertical + 'Scripts/*.js'],
                tasks: ['diff:JS'],
                options: {
                    event: ['added', 'changed'],
                },
            },
            sharedJS: {
                files: ['SharedUI/Scripts/**/*.js'],
                tasks: ['diff:sharedJS'],
                options: {
                    event: ['added', 'changed'],
                },
            },
            sharedSass: {
                files: ['SharedUI/sass/*.scss', 'SharedUI/sass/*/*.scss'],
                tasks: ['doCSS', 'diff:doCSS', 'checkCSS'],
                options: {
                    event: ['added', 'changed'],
                },
            },
            options: {
                // Sets livereload to true for livereload to work 
                // (livereload is not covered in this article)
                //livereload: true,
                spawn: false
            }
        },
        //hash
        hash: {
            options: {
                mapping: 'assetMaps.json', //mapping file so your server can serve the right files 
                flatten: false, // Set to true if you don't want to keep folder structure in the `key` value in the mapping file 
                hashLength: 16, // hash length, the max value depends on your hash function 
                hashFunction: function (source, encoding) { // default is md5 
                    return grunt.template.today('yyyy-mm-dd.HH.MM.ss');
                }
            },
            commonJS: {
                src: [vertical + 'Scripts/bundle/common.js'], //all your js that needs a hash appended to it 
                dest: scriptsPath //where the new files will be created 
            },
            verticalJS: {
                src: [vertical + 'Scripts/bundle/vertical.js'], //all your js that needs a hash appended to it 
                dest: scriptsPath //where the new files will be created 
            },
            css: {
                src: [vertical + 'CSS/bundle/site.css', vertical + 'CSS/bundle/site_[0-9].css'],  //all your css that needs a hash appended to it 
                dest: cssPath //where the new files will be created 
            }
        }

    });

    grunt.loadNpmTasks('grunt-contrib-watch');
    grunt.loadNpmTasks("grunt-csssplit");
    grunt.loadNpmTasks('grunt-sass');
    grunt.loadNpmTasks('grunt-contrib-uglify');
    grunt.loadNpmTasks('grunt-contrib-cssmin');
    grunt.loadNpmTasks('grunt-hash');
    grunt.loadNpmTasks('grunt-contrib-clean');
    grunt.loadNpmTasks('grunt-shell');
    grunt.loadNpmTasks('grunt-force-task');
    grunt.loadNpmTasks('grunt-sync');
    grunt.loadNpmTasks('grunt-diff');

    grunt.registerTask('enableBundle', 'enable bundle', function () {
        activeCSSBundle = true;
        console.log("enabling activeCSSBundle set to: " + activeCSSBundle);
    });

    grunt.registerTask('checkCSS', 'remove unused CSS if needed', function () {
        console.log("value activeCSSBundle: " + activeCSSBundle);
        if (!activeCSSBundle) {
            grunt.task.run('force:shell:undoCSS');
        }
        activeCSSBundle = false;
        console.log("enabling activeCSSBundle set to: " + activeCSSBundle);
    });

    // Compiles Sass into CSS and make the IE split
    grunt.registerTask('doCSS', ['force:shell:checkoutCSS', 'sass', 'csssplit']);

    // Do all bundles
    grunt.registerTask('doBundles', ['force:shell:checkoutbundles', 'clean:all', 'cssmin:sitecss', 'cssmin:IEcss', 'cssmin:responsivecss', 'uglify:Common', 'uglify:Vertical']);

    // Do all legacy bundles
    grunt.registerTask('doLegacyBundles', ['force:shell:checkoutlegacybundles', 'clean:legacyall', 'cssmin:legacysitecss1', 'cssmin:legacysitecss2', 'cssmin:legacysitecss3', 'uglify:LegacyCommon', 'uglify:LegacyVertical']);

    // Do the common.js bundle
    grunt.registerTask('doCommonJSBundle', [
        'force:shell:checkoutCommonJS',
        'uglify:Common',
        'force:shell:removeOldFiles:common',
        'force:shell:checkoutMapJSON',
        'clean:hashCommonJS',
        'hash:commonJS',
        'updateFileInclude:common',
        'force:shell:reconcileHash:' + path.resolve(scriptsPath)
    ]);

    // Do the vertical.js bundle
    grunt.registerTask('doVerticalJSBundle', [
        'force:shell:checkoutVerticalJS',
        'uglify:Vertical',
        'force:shell:removeOldFiles:vertical',
        'force:shell:checkoutMapJSON',
        'clean:hashVerticalJS',
        'hash:verticalJS',
        'updateFileInclude:vertical',
        'force:shell:reconcileHash:' + path.resolve(scriptsPath)
    ]);

    // Do all CSS bundles
    grunt.registerTask('doCSSBundle', [
        'enableBundle',
        'force:shell:checkoutCSSbundles',
        'cssmin:sitecss',
        'cssmin:IEcss',
        'force:shell:removeOldFiles:css',
        'force:shell:checkoutMapJSON',
        'clean:hashCSS',
        'hash:css',
        'updateFileInclude:css',
        'force:shell:reconcileHash:' + path.resolve(cssPath)
    ]);

    grunt.registerTask('updateFileInclude',
        '=== Updates the css inclusion partial view so it calls adds cache busting to the query ===',
        function (file) {
            var cshtmlPath;
            var includeLine = '';
            var json = grunt.file.readJSON('assetMaps.json');

            if (file === 'common' || file == 'vertical') {
                cshtmlPath = file === 'common' ? 'Views/Shared/_commonjs.cshtml' : 'Views/Shared/_verticaljs.cshtml';
                formsPath = file === 'common' ? 'Controls/Template/commonjs.ascx' : 'Controls/Template/verticaljs.ascx';
                includeLine = '<script type="text/javascript" src="/' + (file === 'common' ? json[vertical + 'Scripts/bundle/common.js'] : json[vertical + 'Scripts/bundle/vertical.js']) + '"></script>';
                grunt.task.run("force:shell:addNewFiles:" + (file === 'common' ? json[vertical + 'Scripts/bundle/common.js'] : json[vertical + 'Scripts/bundle/vertical.js']));
            } else if (file === 'css') {
                cshtmlPath = 'Views/Shared/_verticalCSS.cshtml';
                formsPath = "Controls/Template/verticalCSS.ascx";
                for (var key in json) {
                    if ((key && typeof key === 'string' && /site\.css$/i.test(key))) {
                        includeLine += '<!--[if gt IE 9]> -->\r\n';
                        includeLine += '<link rel="stylesheet" type="text/css" href="/' + json[key] + '" />' + "\r\n";
                        includeLine += '<!-- <![endif]-->\r\n';
                        grunt.task.run("force:shell:addNewFiles:" + json[key]);
                    }
                }
                includeLine += '<!--[if lte IE 9]>\r\n';
                for (var key in json) {
                    if (key && typeof key === 'string' && /site_[0-9]\.css$/i.test(key)) {
                        includeLine += '<link rel="stylesheet" type="text/css" href="/' + json[key] + '" />' + "\r\n";
                        grunt.task.run("force:shell:addNewFiles:" + json[key]);
                    }
                }
                includeLine += '<![endif]-->';
            }
            grunt.file.write(cshtmlPath, includeLine);
            grunt.file.write(formsPath, includeLine);
            console.log("=== Updated cache-busting ===");
            console.log(includeLine);
        });


};