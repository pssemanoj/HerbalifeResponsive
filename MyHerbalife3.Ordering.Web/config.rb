#require 'compass/import-once/activate'
# Require any additional compass plugins here.

require "normalize-scss";
require "susy";
require "breakpoint";



# Set this to the root of your project when deployed:
http_path = "/"
css_dir = "Ordering/CSS"
sass_dir = "Ordering/sass"
images_dir = "Ordering/images"
javascripts_dir = "Ordering/Scripts"
sass_options = {:sourcemap => true}
fonts_dir = "SharedUI/CSS/fonts/"
http_fonts_dir = "SharedUI/CSS/fonts/"

# Set this to true to silence deprecation warnings.
disable_warnings = true

# You can select your preferred output style here (can be overridden via the command line):
# output_style = :expanded or :nested or :compact or :compressed

# To enable relative paths to assets via compass helper functions. Uncomment:
# relative_assets = true

# To disable debugging comments that display the original location of your selectors. Uncomment:
line_comments = false


# If you prefer the indented syntax, you might want to regenerate this
# project again passing --syntax sass, or you can uncomment this:
# preferred_syntax = :sass
# and then run:
# sass-convert -R --from scss --to sass sass scss && rm -rf sass && mv scss sass

# This one split files into max selectors length of 4095 for IE
class CssSplitter

  def self.split(infile, outdir = File.dirname(infile), max_selectors = 4095)

    raise "infile could not be found" unless File.exists? infile

    rules = IO.readlines(infile, "}")
    return if rules.first.nil?

    charset_statement, rules[0] = rules.first.partition(/^\@charset[^;]+;/)[1,2]
    return if rules.nil?

    # The infile remains the first file
    file_id = 0
    selectors_count = 4095
    output = nil

    rules.each do |rule|
      rule_selectors_count = count_selectors_of_rule rule
      selectors_count += rule_selectors_count

      # Nothing happens until the selectors limit is reached for the first time
      if selectors_count > max_selectors
        # Close current file if there is already one
        output.close if output

        # Prepare next file
        file_id += 1
        filename = File.join(outdir, File.basename(infile, File.extname(infile)) + "_#{file_id.to_s}" + File.extname(infile))
        output = File.new(filename, "w")
        output.write charset_statement

        # Reset count with current rule count
        selectors_count = rule_selectors_count
      end

      output.write rule if output
    end
  end

  def self.count_selectors(css_file)
    raise "file could not be found" unless File.exists? css_file

    rules = IO.readlines(css_file, '}')
    return if rules.first.nil?

    charset_statement, rules[0] = rules.first.partition(/^\@charset[^;]+;/)[1,2]
    return if rules.first.nil?

    rules.inject(0) {|count, rule| count + count_selectors_of_rule(rule)}.tap do |result|
      puts File.basename(css_file) + " contains #{result} selectors."
    end
  end

  def self.count_selectors_of_rule(rule)
    rule.partition(/\{/).first.scan(/,/).count.to_i + 1
  end
  
end

on_stylesheet_saved do |path|
  CssSplitter.split(path) unless path[/\d+$/]
end
